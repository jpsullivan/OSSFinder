using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using StackExchange.Profiling;

namespace OSSFinder
{
    /// <summary>
    /// Helper class that provides quick access to common objects used across a single request.
    /// </summary>
    public class Current
    {
        #region IoC

        public Current() { }

        #endregion

        private static DeploymentTier? tier;
        private const string DisposeConnectionKey = "dispose_connections";

        public static void RegisterConnectionForDisposal(DbConnection connection)
        {
            if (Context != null)
            {
                var connections = Context.Items[DisposeConnectionKey] as List<DbConnection>;
                if (connections == null)
                {
                    Context.Items[DisposeConnectionKey] = connections = new List<DbConnection>();
                }

                connections.Add(connection);
            }
        }

        public static void DisposeRegisteredConnections()
        {
            var connections = Context.Items[DisposeConnectionKey] as List<DbConnection>;
            if (connections == null)
            {
                return;
            }

            Context.Items[DisposeConnectionKey] = null;

            foreach (var connection in connections)
            {
                try
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        GlobalApplication.LogException("Connection was not in a closed state.");
                    }

                    connection.Dispose();
                }
                catch
                {
                    /* don't care, nothing we can do */
                }
            }
        }

        /// <summary>
        /// Shortcut to HttpContext.Current.
        /// </summary>
        public static HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        /// <summary>
        /// Shortcut to HttpContext.Current.Request.
        /// </summary>
        public static HttpRequest Request
        {
            get { return Context.Request; }
        }

        /// <summary>
        /// Gets the controller for the current request; should be set during init of current request's controller.
        /// </summary>
        public BaseController Controller
        {
            get { return Context.Items["Controller"] as BaseController; }
            set { Context.Items["Controller"] = value; }
        }

        /// <summary>
        /// Gets the current "authenticated" user from this request's controller.
        /// </summary>
        public User User
        {
            get
            {
                if (Controller == null)
                {
                    var bc = new BaseController();
                    return bc.GetCurrentUser(HttpContext.Current.Request, HttpContext.Current.User.Identity);
                }
                return Controller.CurrentUser;
            }
        }

        /// <summary>
        /// Determines whether or not a profile-specific 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasCustomLogo()
        {
            if (FileStorageService == null)
                FileStorageService = Container.Kernel.TryGet<FileSystemFileStorageService>();

            return await FileStorageService.FileExistsAsync("logos", "logo-" + this.Controller.CurrentProfile.ProfileId + ".png");
        }

        public bool HasSystemWideLogo()
        {
            return false;
        }

        public async Task<string> GetCustomLogoUrl(int profileId = 0)
        {
            if (profileId == 0)
            {
                // should probably implement a system-wide exception for this
                profileId = this.Controller.CurrentProfile.ProfileId;
            }

            if (FileStorageService == null)
                FileStorageService = Container.Kernel.TryGet<FileSystemFileStorageService>();

            var fileName = "logo-" + profileId + ".png";
            if (await FileStorageService.FileExistsAsync("logos", fileName))
            {
                return Path.Combine(this.Controller.Url.SiteRoot(), "Uploads", "logos", fileName);
            }

            return "";
        }

        private class ErrorLoggingProfiler : StackExchange.Profiling.Data.IDbProfiler
        {
            private readonly StackExchange.Profiling.Data.IDbProfiler wrapped;

            public ErrorLoggingProfiler(StackExchange.Profiling.Data.IDbProfiler wrapped)
            {
                this.wrapped = wrapped;
            }

            public void ExecuteFinish(IDbCommand profiledDbCommand, StackExchange.Profiling.Data.ExecuteType executeType,
                                      DbDataReader reader)
            {
                wrapped.ExecuteFinish(profiledDbCommand, executeType, reader);
            }

            public void ExecuteStart(IDbCommand profiledDbCommand, StackExchange.Profiling.Data.ExecuteType executeType)
            {
                wrapped.ExecuteStart(profiledDbCommand, executeType);
            }

            public bool IsActive
            {
                get { return wrapped.IsActive; }
            }

            public void OnError(IDbCommand profiledDbCommand, StackExchange.Profiling.Data.ExecuteType executeType,
                                Exception exception)
            {
                var formatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
                var timing = new SqlTiming(profiledDbCommand, executeType, null);
                exception.Data["SQL"] = formatter.FormatSql(timing);
                wrapped.OnError(profiledDbCommand, executeType, exception);
            }

            public void ReaderFinish(IDataReader reader)
            {
                wrapped.ReaderFinish(reader);
            }
        }

        /// <summary>
        /// Gets the single data context for this current request.
        /// </summary>
        public static IntraDatabase DB
        {
            get
            {
                IntraDatabase result;
                if (Context != null)
                {
                    result = Context.Items["DB"] as IntraDatabase;
                }
                else
                {
                    // unit tests
                    result = CallContext.GetData("DB") as IntraDatabase;
                }

                if (result != null)
                    return result;

                DbConnection cnn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SQL"].ConnectionString);
                if (Profiler != null)
                {
                    cnn = new StackExchange.Profiling.Data.ProfiledDbConnection(cnn, new ErrorLoggingProfiler(Profiler));
                }
                cnn.Open();
                RegisterConnectionForDisposal(cnn);
                result = IntraDatabase.Init(cnn, 30);

                if (Context != null)
                {
                    Context.Items["DB"] = result;
                }
                else
                {
                    CallContext.SetData("DB", result);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets where this code is running, e.g. Prod, Dev
        /// </summary>
        public static DeploymentTier Tier
        {
            get
            {
                if (!tier.HasValue)
                {
                    tier = DeploymentTier.Local;
                }
                //_tier = (DeploymentTier) Enum.Parse(typeof(DeploymentTier), Site.Tier, true);

                return tier.Value;
            }
        }

        /// <summary>
        /// Allows end of reqeust code to clean up this request's DB.
        /// </summary>
        public static void DisposeDB()
        {
            IntraDatabase db;
            if (Context != null)
            {
                db = Context.Items["DB"] as IntraDatabase;
            }
            else
            {
                db = CallContext.GetData("DB") as IntraDatabase;
            }

            if (db == null)
            {
                return;
            }

            db.Dispose();
            if (Context != null)
            {
                Context.Items["DB"] = null;
            }
            else
            {
                CallContext.SetData("DB", null);
            }
        }

        /// <summary>
        /// retrieve an integer from the HttpRuntime.Cache; returns 0 if value does not exist
        /// </summary>
        public static int GetCachedInt(string key)
        {
            object o = HttpRuntime.Cache[key];
            if (o == null)
            {
                return 0;
            }
            return (int)o;
        }

        /// <summary>
        /// remove a cached object from the HttpRuntime.Cache
        /// </summary>
        public static void RemoveCachedObject(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        /// <summary>
        /// retrieve an object from the HttpRuntime.Cache
        /// </summary>
        public static object GetCachedObject(string key)
        {
            return HttpRuntime.Cache[key];
        }

        /// <summary>
        /// add an object to the HttpRuntime.Cache with an absolute expiration time
        /// </summary>
        public static void SetCachedObject(string key, object o, int durationSecs)
        {
            HttpRuntime.Cache.Add(
                key,
                o,
                null,
                DateTime.Now.AddSeconds(durationSecs),
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                null);
        }

        /// <summary>
        /// add an object to the HttpRuntime.Cache with a sliding expiration time
        /// </summary>
        public static void SetCachedObjectSliding(string key, object o, int slidingSecs)
        {
            HttpRuntime.Cache.Add(
                key,
                o,
                null,
                Cache.NoAbsoluteExpiration,
                new TimeSpan(0, 0, slidingSecs),
                CacheItemPriority.High,
                null);
        }

        /// <summary>
        /// add a non-removable, non-expiring object to the HttpRuntime.Cache
        /// </summary>
        public static void SetCachedObjectPermanent(string key, object o)
        {
            HttpRuntime.Cache.Remove(key);
            HttpRuntime.Cache.Add(
                key,
                o,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);
        }

        /// <summary>
        /// retrieves a string from the HttpContext.Cache, or null if the key doesn't exist
        /// </summary>
        public static string GetCachedString(string key)
        {
            object o = HttpRuntime.Cache[key];
            if (o != null)
            {
                return o.ToString();
            }
            return null;
        }

        /// <summary>
        /// places a string in the HttpContext.Cache
        /// cached with "sliding expiration", so will only be deleted if NOT accessed for durationSecs
        /// </summary>
        public static void SetCachedString(string key, int durationSecs, string s)
        {
            HttpRuntime.Cache.Add(
                key,
                s,
                null,
                DateTime.MaxValue,
                TimeSpan.FromSeconds(durationSecs),
                CacheItemPriority.High,
                null);
        }

        /// <summary>
        /// manually write a message (wrapped in a simple Exception) to our standard exception log
        /// </summary>
        public static void LogException(string message)
        {
            GlobalApplication.LogException(message);
        }

        /// <summary>
        /// manually write an exception to our standard exception log
        /// </summary>
        public static void LogException(Exception ex)
        {
            GlobalApplication.LogException(ex);
        }

        public static MiniProfiler Profiler
        {
            get { return MiniProfiler.Current; }
        }
    }

    public enum DeploymentTier
    {
        Prod,
        Dev,
        Local
    }
}