using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using OSSFinder.Core.Entities;
using OSSFinder.Infrastructure.Helpers;
using OSSFinder.Services;

namespace OSSFinder.Infrastructure.Extensions
{
    public static class ExtensionMethods
    {
        private static readonly Crc16 Crc16 = new Crc16();

        private static readonly Regex GuidFormat = new Regex(
            @"^[A-Fa-f0-9]{32}$|
                  ^({|\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\))?$|
                  ^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        ///     returns a hash of the time that is stable for up to 16 minutes (999 seconds)
        /// </summary>
        public static string GetHashedTime()
        {
            // one tick is 10ns; ticks are 18 characters long
            return ("ticks=" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture).Substring(0, 8)).ToCrc16();
        }

        /// <summary>
        ///     Counts the number of occurences of the search string in this string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static int OccurencesOf(this string s, string search)
        {
            int index = s.IndexOf(search, StringComparison.Ordinal), count = 0;

            while (index > -1)
            {
                ++count;

                index = s.IndexOf(search, index + search.Length, StringComparison.Ordinal);
            }

            return count;
        }

        /// <summary>
        /// Answers true if this String is either null or empty.
        /// </summary>
        /// <remarks>I'm so tired of typing String.IsNullOrEmpty(s)</remarks>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        ///     Returns the first non-null/non-empty parameter when this String is null/empty.
        /// </summary>
        public static string IsNullOrEmptyReturn(this string s, params string[] otherPossibleResults)
        {
            if (s.HasValue())
            {
                return s;
            }

            for (var i = 0; i < (otherPossibleResults ?? new string[0]).Length; i++)
            {
                if (otherPossibleResults != null && otherPossibleResults[i].HasValue())
                {
                    return otherPossibleResults[i];
                }
            }

            return "";
        }

        /// <summary>
        ///     returns string with any mulitple, sequential spaces replaced by a single space,
        ///     and any extra spaces trimmed from beginning and end.
        /// </summary>
        public static string RemoveExtraSpaces(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return s;
            }
            s = Regex.Replace(s, "\u200c", " "); // see http://en.wikipedia.org/wiki/Zero-width_non-joiner
            s = Regex.Replace(s, @"\s{2,}", " ").Trim();
            return s;
        }

        /// <summary>
        ///     Produces a URL-friendly version of this String, "like-this-one".
        /// </summary>
        public static string UrlFriendly(this string s)
        {
            return s.HasValue() ? UrlFriendly(s) : s;
        }

        /// <summary>
        ///     Produces a URL-friendly version of this String, "like-this-one", and prepends it with
        ///     a forward slash if the URL-friendly version is non-blank
        /// </summary>
        public static string Slugify(this string s, bool containsForwardSlash = true)
        {
            if (!s.HasValue())
            {
                return s;
            }

            string slug = UrlFriendly(s);
            return slug.HasValue() ? "/" + slug : "";
        }

        /// <summary>
        ///     returns Url Encoded string
        /// </summary>
        public static string UrlEncode(this string s)
        {
            return s.HasValue() ? HttpUtility.UrlEncode(s) : s;
        }

        /// <summary>
        ///     returns Html Encoded string
        /// </summary>
        public static string HtmlEncode(this string s)
        {
            return s.HasValue() ? HttpUtility.HtmlEncode(s) : s;
        }

        /// <summary>
        ///     returns true if this looks like a semi-valid email address
        /// </summary>
        public static bool IsEmailAddress(this string s)
        {
            return s.HasValue() && Regex.IsMatch(s, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        ///     returns true if this looks like a semi-valid openid string; it starts with "=@+$!(" or contains a period.
        /// </summary>
        public static bool IsOpenId(this string s)
        {
            return s.HasValue() && Regex.IsMatch(s, @"^[=@+$!(]|.*?\.");
        }

        public static bool AnySafe<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return false;
            }
            return items.Any();
        }

        public static bool AnySafe<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null)
            {
                return false;
            }
            return items.Any(predicate);
        }

        /// <summary>
        ///     Adds the url to the RouteCollection with the specified defaults.
        /// </summary>
        /// <remarks>Added to remove annoying empty space in all MapRoute calls.</remarks>
        public static void MapRoute(this RouteCollection routes, string url, object defaults)
        {
            routes.MapRoute("", url, defaults);
        }

        public static string Temperature(this DateTime dt)
        {
            TimeSpan ts = DateTime.UtcNow - dt;
            double delta = ts.TotalMinutes;

            if (delta < 10)
            {
                return "supernova";
            }
            return delta < 120 ? "warm" : "cool";
        }

        public static string Temperature(this DateTime? dt)
        {
            if (dt == null)
            {
                return "cool";
            }
            return Temperature(dt.Value);
        }

        /// <summary>
        ///     Returns a unix Epoch time given a Date
        /// </summary>
        public static long ToJavascriptTime(this DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        /// <summary>
        ///     Converts to Date given an Epoch time
        /// </summary>
        public static DateTime ToDateTime(this long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(epoch);
        }

        /// <summary>
        ///     Returns a humanized string indicating how long ago something happened, eg "3 days ago".
        ///     For future dates, returns when this DateTime will occur from DateTime.UtcNow.
        /// </summary>
        public static string ToRelativeTime(this DateTime dt)
        {
            DateTime utcNow = DateTime.UtcNow;
            return dt <= utcNow ? ToRelativeTimePast(dt, utcNow) : ToRelativeTimeFuture(dt, utcNow);
        }

        /// <summary>
        ///     Returns a humanized string indicating how long ago something happened, eg "3 days ago".
        ///     For future dates, returns when this DateTime will occur from DateTime.UtcNow.
        ///     If this DateTime is null, returns empty string.
        /// </summary>
        public static string ToRelativeTime(this DateTime? dt)
        {
            if (dt == null)
            {
                return "";
            }
            return ToRelativeTime(dt.Value);
        }

        private static string ToRelativeTimePast(DateTime dt, DateTime utcNow)
        {
            var ts = utcNow - dt;
            var delta = ts.TotalSeconds;

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "1 sec ago" : ts.Seconds + " secs ago";
            }
            if (delta < 3600) // 60 mins * 60 sec
            {
                return ts.Minutes == 1 ? "1 min ago" : ts.Minutes + " mins ago";
            }
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
            {
                return ts.Hours == 1 ? "1 hour ago" : ts.Hours + " hours ago";
            }

            var days = ts.Days;
            if (days == 1)
            {
                return "yesterday";
            }
            if (days <= 2)
            {
                return days + " days ago";
            }
            return dt.ToString(utcNow.Year == dt.Year ? "MMM %d 'at' %H:mmm" : @"MMM %d \'yy 'at' %H:mmm");
        }

        private static string ToRelativeTimeFuture(DateTime dt, DateTime utcNow)
        {
            var ts = dt - utcNow;
            var delta = ts.TotalSeconds;

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "in 1 second" : "in " + ts.Seconds + " seconds";
            }
            if (delta < 3600) // 60 mins * 60 sec
            {
                return ts.Minutes == 1 ? "in 1 minute" : "in " + ts.Minutes + " minutes";
            }
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
            {
                return ts.Hours == 1 ? "in 1 hour" : "in " + ts.Hours + " hours";
            }

            // use our own rounding so we can round the correct direction for future
            var days = (int)Math.Round(ts.TotalDays, 0);
            if (days == 1)
            {
                return "tomorrow";
            }
            if (days <= 10)
            {
                return "in " + days + " day" + (days > 1 ? "s" : "");
            }
            // if the date is in the future enough to be in a different year, display the year
            if (utcNow.Year != dt.Year)
            {
                return "on " + dt.ToString(@"MMM %d \'yy 'at' %H:mmm");
            }
            return "on " + dt.ToString("MMM %d 'at' %H:mmm");
        }

        /// <summary>
        ///     returns a html span element with relative time elapsed since this event occurred, eg, "3 months ago" or "yesterday";
        ///     assumes time is *already* stored in UTC format!
        /// </summary>
        public static string ToRelativeTimeSpan(this DateTime dt)
        {
            return ToRelativeTimeSpan(dt, "relativetime");
        }

        public static string ToRelativeTimeSpan(this DateTime dt, string cssclass)
        {
            if (cssclass == null)
            {
                return string.Format(@"<span title=""{0:u}"">{1}</span>", dt, ToRelativeTime(dt));
            }
            return string.Format(@"<span title=""{0:u}"" class=""{2}"">{1}</span>", dt, ToRelativeTime(dt), cssclass);
        }

        public static string ToRelativeTimeSpan(this DateTime? dt)
        {
            if (dt == null)
            {
                return "";
            }
            return ToRelativeTimeSpan(dt.Value);
        }


        /// <summary>
        ///     returns a very *small* humanized string indicating how long ago something happened, eg "3d ago"
        /// </summary>
        public static string ToRelativeTimeMini(this DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            var delta = ts.TotalSeconds;

            if (delta < 60)
            {
                return ts.Seconds + "s ago";
            }
            if (delta < 3600) // 60 mins * 60 sec
            {
                return ts.Minutes + "m ago";
            }
            if (delta < 86400) // 24 hrs * 60 mins * 60 sec
            {
                return ts.Hours + "h ago";
            }
            var days = ts.Days;
            if (days <= 2)
            {
                return days + "d ago";
            }
            if (days <= 330)
            {
                return dt.ToString("MMM %d 'at' %H:mmm").ToLowerInvariant();
            }
            return dt.ToString(@"MMM %d \'yy 'at' %H:mmm").ToLowerInvariant();
        }


        public static string ToRelativeTimeMicro(this DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);

            if (ts.Days <= 330)
            {
                return dt.ToString("MMM %d").ToLower();
            }
            return dt.ToString("MMM %d yy").ToLower();
        }

        /// <summary>
        ///     returns AN HTML SPAN ELEMENT with minified relative time elapsed since this event occurred, eg, "3mo ago" or "yday";
        ///     assumes time is *already* stored in UTC format!
        /// </summary>
        public static string ToRelativeTimeSpanMini(this DateTime dt)
        {
            return string.Format(@"<span title=""{0:u}"" class=""relativetime"">{1}</span>", dt, ToRelativeTimeMini(dt));
        }

        /// <summary>
        ///     returns AN HTML SPAN ELEMENT with minified relative time elapsed since this event occurred, eg, "3mo ago" or "yday";
        ///     assumes time is *already* stored in UTC format!
        ///     If this DateTime? is null, will return empty string.
        /// </summary>
        public static string ToRelativeTimeSpanMini(this DateTime? dt)
        {
            if (dt == null)
            {
                return "";
            }
            return ToRelativeTimeSpanMini(dt.Value);
        }

        public static IHtmlString AsHtml(this string html)
        {
            return MvcHtmlString.Create(html);
        }

        public static IHtmlString ToRelativeTimeSpanMicro(this DateTime dt)
        {
            return
                string.Format(@"<span title=""{0:u}"" class=""relativetime"">{1}</span>", dt, ToRelativeTimeMicro(dt))
                      .AsHtml();
        }

        public static IHtmlString ToRelativeTimeSpanMicro(this DateTime? dt)
        {
            if (dt == null)
            {
                return "".AsHtml();
            }
            return ToRelativeTimeSpanMicro(dt.Value);
        }


        public static string ToAtomFeedDate(this DateTime dt)
        {
            return string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", dt);
        }

        public static string ToAtomFeedDate(this DateTime? dt)
        {
            return dt == null ? "" : ToAtomFeedDate(dt.Value);
        }


        /// <summary>
        ///     returns how long something took in sec, minutes, hours, or days
        /// </summary>
        public static string TimeTaken(this TimeSpan time)
        {
            string output = "";
            if (time.Days > 0)
            {
                output += time.Days + " day" + (time.Days > 1 ? "s " : " ");
            }
            if ((time.Days == 0 || time.Days == 1) && time.Hours > 0)
            {
                output += time.Hours + " hour" + (time.Hours > 1 ? "s " : " ");
            }
            if (time.Days == 0 && time.Minutes > 0)
            {
                output += time.Minutes + " minute" + (time.Minutes > 1 ? "s " : " ");
            }
            if (output.Length == 0)
            {
                output += time.Seconds + " second" + (time.Seconds > 1 ? "s " : " ");
            }
            return output.Trim();
        }

        /// <summary>
        ///     returns how long something took in years, months, or days
        /// </summary>
        public static string TimeTakenLong(this DateTime dt)
        {
            int days = (DateTime.UtcNow - dt).Days;
            if (days <= 0)
            {
                return "today";
            }
            if (days <= 1)
            {
                return "yesterday";
            }
            if (days > 365)
            {
                return (days / 365) + " year" + ((days / 365) > 1 ? "s ago" : " ago");
            }
            if (days > 30)
            {
                return (days / 30) + " month" + ((days / 30) > 1 ? "s ago" : " ago");
            }
            return days + " day" + (days > 1 ? "s ago" : " ago");
        }

        /// <summary>
        ///     returns how long something took in years, months, or days
        /// </summary>
        public static string TimeTakenLong(this DateTime? dt)
        {
            if (dt == null)
            {
                return "";
            }
            return TimeTakenLong(dt.Value);
        }


        public static string ToCrc16(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return "";
            }

            byte[] crc = Crc16.ComputeChecksumBytes(Encoding.UTF8.GetBytes(s));
            return crc[0].ToString("x2") + crc[1].ToString("x2");
        }

        public static string ToMD5Hash(this string s)
        {
            return ToHash(MD5.Create, s);
        }

        public static string ToSha256Hash(this string s)
        {
            return ToHash(SHA256.Create, s);
        }

        private static string ToHash(Func<HashAlgorithm> createMethod, string toHash)
        {
            if (toHash.IsNullOrEmpty())
            {
                return "";
            }

            byte[] hash;
            using (HashAlgorithm algorithm = createMethod())
            {
                hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            }
            // hex encoding yields 2 char/byte -> 255 == FF == 1111 1111
            var result = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }


        public static string Pluralize(this string word, int number)
        {
            // http://meta.stackoverflow.com/questions/61380/inflector-net-not-correctly-attributed-to-andrew-peters-in-stack-exchange-data-ex
            return (number == 1) ? word : word + "s";
        }

        public static string PrettyShort(this int? num)
        {
            if (num == null)
            {
                return "";
            }

            return num.Value.PrettyShort();
        }

        public static string PrettyShort(this int num)
        {
            string rval;

            if (num < 1000)
            {
                rval = num.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var divisor = num < 1000000 ? 1000.0 : 1000000.0;

                rval = ((int)Math.Round(num / divisor)).ToString(CultureInfo.InvariantCulture).Length > 1 ?
                    (Math.Round(num / divisor)).ToString(CultureInfo.InvariantCulture) : (Math.Round(num / (divisor / 10.0)) / 10.0).ToString(CultureInfo.InvariantCulture);
            }

            if (rval.Length > 3)
            {
                rval = rval.Substring(0, 3);
            }

            while (rval.Contains(".") && rval.Last() == '0')
            {
                rval = rval.Substring(0, rval.Length - 1);
            }

            rval = rval.Last() == '.' ? rval.Substring(0, rval.Length - 1) : rval;

            var suffix = "";
            if (num < 1000)
            {
                suffix = "";
            }
            else if (num > 999 && num < 1000000)
            {
                suffix = "k";
            }
            else if (num >= 1000000)
            {
                suffix = "m";
            }

            return string.Format("<span class=\"pretty-short\" title=\"{0}\">{1}{2}</span>", Pretty(num), rval, suffix);
        }

        public static string Pretty(this Int32? num)
        {
            if (num == 0 || num == null)
            {
                return "";
            }
            return num.Value.ToString("#,##0");
        }


        public static bool GuidTryParse(this string s, out Guid result)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (GuidFormat.IsMatch(s))
            {
                result = new Guid(s);
                return true;
            }
            result = Guid.Empty;
            return false;
        }

        public static void SetPageTitle(this WebViewPage page, string title)
        {
            title = HtmlHelpers.Encode(title);
            page.ViewData["PageTitle"] = title;
        }

        #region NameValueCollection (aka Request.Form) Get Helpers

        public static List<T> GetList<T>(this NameValueCollection nvc, string key, char delimiter)
        {
            string[] values = Get<string>(nvc, key).Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<T>(values.Length);
            Type tType = typeof(T);

            if (tType == typeof(string))
            {
                result.AddRange((IEnumerable<T>)(object)values);
            }
            else
            {
                foreach (string s in values)
                {
                    try
                    {
                        result.Add((T)Convert.ChangeType(s.Trim(), tType));
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException(
                            string.Format("Unable to convert \"{0}\" into a {1}.", s, tType.FullName), ex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Answers true if a value is found for 'key'
        /// </summary>
        public static bool Contains(this NameValueCollection nvc, string key)
        {
            return !string.IsNullOrEmpty(nvc[key]);
        }

        public static T Get<T>(this NameValueCollection nvc, string key)
        {
            return Get(nvc, key, default(T), false);
        }

        public static T Get<T>(this NameValueCollection nvc, string key, T defaultValue)
        {
            return Get(nvc, key, defaultValue, false);
        }

        private static T Get<T>(NameValueCollection nvc, string key, T defaultValue, bool throwExceptionWhenValueIsEmpty)
        {
            T result = defaultValue;
            Type resultType = typeof(T);

            string value = nvc[key];

            if (string.IsNullOrEmpty(value))
            {
                if (throwExceptionWhenValueIsEmpty)
                {
                    throw new HttpRequestValidationException(string.Format(
                        "Unable to find a Form value for key '{0}'.", key));
                }
            }
            else
            {
                try
                {
                    // Some controls submit "on" when checked or selected..
                    if (resultType == typeof(bool) && value.Equals("on", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "True";
                    }

                    if (resultType == typeof(string))
                    {
                        // don't trim strings here; this breaks markdown parsing, as leading spaces can be significant
                        result = (T)Convert.ChangeType(HtmlHelpers.RemoveInvalidXmlChars(value), resultType);
                    }
                    else if (resultType.IsEnum)
                    {
                        result = (T)Enum.Parse(resultType, value);
                    }
                    else
                    {
                        result = (T)Convert.ChangeType(value.Trim(), resultType);
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpRequestValidationException(string.Format(
                        "Encountered a problem trying to convert '{0}' to a {1}.", value, resultType.FullName),
                                                             ex);
                }
            }

            return result;
        }

        #endregion

        public static void AddOrSet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key, TValue val)
        {
            self.AddOrUpdate(key, val, (_, __) => val);
        }

        public static SecureString ToSecureString(this string str)
        {
            SecureString output = new SecureString();
            foreach (char c in str)
            {
                output.AppendChar(c);
            }
            output.MakeReadOnly();
            return output;
        }

        public static string ToStringOrNull(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.ToString();
        }

        public static string ToEncodedUrlStringOrNull(this Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            return uri.AbsoluteUri;
        }

        public static string ToStringSafe(this object obj)
        {
            if (obj != null)
            {
                return obj.ToString();
            }
            return String.Empty;
        }

        public static string Flatten(this IEnumerable<string> list)
        {
            if (list == null)
            {
                return String.Empty;
            }

            return String.Join(", ", list.ToArray());
        }

        public static bool IsInThePast(this DateTime? date)
        {
            return date.Value.IsInThePast();
        }

        public static bool IsInThePast(this DateTime date)
        {
            return date < DateTime.UtcNow;
        }

        public static MailAddress ToMailAddress(this User user)
        {
            return new MailAddress(user.EmailAddress, user.Username);
        }

        public static string GetClaimOrDefault(this ClaimsPrincipal self, string claimType)
        {
            return self.Claims.GetClaimOrDefault(claimType);
        }

        public static string GetClaimOrDefault(this ClaimsIdentity self, string claimType)
        {
            return self.Claims.GetClaimOrDefault(claimType);
        }

        public static string GetClaimOrDefault(this IEnumerable<Claim> self, string claimType)
        {
            return self
                .Where(c => String.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .FirstOrDefault();
        }

        // This is a method because the first call will perform a database call
        /// <summary>
        /// Get the current user, from the database, or if someone in this request has already
        /// retrieved it, from memory. This will NEVER return null. It will throw an exception
        /// that will yield an HTTP 401 if it would return null. As a result, it should only
        /// be called in actions with the Authorize attribute or a Request.IsAuthenticated check
        /// </summary>
        /// <returns>The current user</returns>
        public static User GetCurrentUser(this IOwinContext self)
        {
            if (self.Request.User == null)
            {
                return null;
            }

            User user = null;
            object obj;
            if (self.Environment.TryGetValue(Constants.CurrentUserOwinEnvironmentKey, out obj))
            {
                user = obj as User;
            }

            if (user == null)
            {
                user = LoadUser(self);
                self.Environment[Constants.CurrentUserOwinEnvironmentKey] = user;
            }

            if (user == null)
            {
                // Unauthorized! If we get here it's because a valid session token was presented, but the
                // user doesn't exist any more. So we just have a generic error.
                throw new HttpException(401, Strings.Unauthorized);
            }

            return user;
        }

        private static User LoadUser(IOwinContext context)
        {
            var principal = context.Authentication.User;
            if (principal != null)
            {
                // Try to authenticate with the user name
                string userName = principal.GetClaimOrDefault(ClaimTypes.Name);

                if (!String.IsNullOrEmpty(userName))
                {
                    return DependencyResolver
                        .Current
                        .GetService<UserService>()
                        .FindByUsername(userName);
                }
            }
            return null; // No user logged in, or credentials could not be resolved
        }
    }
}