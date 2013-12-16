using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;
using Elmah;
using Elmah.Contrib.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.Web.Common;
using OSSFinder;
using OSSFinder.App_Start;
using OSSFinder.Configuration;
using OSSFinder.Infrastructure;
using OSSFinder.Infrastructure.Attributes;
using OSSFinder.Infrastructure.Filters;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(AppActivator), "PreStart")]
[assembly: PostApplicationStartMethod(typeof(AppActivator), "PostStart")]
[assembly: ApplicationShutdownMethod(typeof(AppActivator), "Stop")]

namespace OSSFinder.App_Start
{
    public static class AppActivator
    {
        private static readonly Bootstrapper NinjectBootstrapper = new Bootstrapper();

        public static void PreStart()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(CreateViewEngine());

            NinjectPreStart();
            ElmahPreStart();

            try
            {
                if (RoleEnvironment.IsAvailable)
                {
                    CloudPreStart();
                }
            }
            catch (Exception)
            {
                // Azure SDK not available!
            }
        }

        public static void PostStart()
        {
            // Get configuration from the kernel
            var config = Container.Kernel.Get<IAppConfiguration>();
            AppPostStart();
        }

        public static void Stop()
        {
            NinjectStop();
        }

        private static RazorViewEngine CreateViewEngine()
        {
            var ret = new RazorViewEngine();

            ret.AreaMasterLocationFormats =
                ret.AreaViewLocationFormats =
                ret.AreaPartialViewLocationFormats =
                new string[]
            {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Branding/Views/Shared/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };

            ret.MasterLocationFormats =
                ret.ViewLocationFormats =
                ret.PartialViewLocationFormats =
                new string[]
            {
                "~/Branding/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.cshtml",
                "~/Branding/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
            };

            return ret;
        }

        private static void CloudPreStart()
        {
            Trace.Listeners.Add(new DiagnosticMonitorTraceListener());
        }

        private static void ElmahPreStart()
        {
            ServiceCenter.Current = _ => Container.Kernel;
        }

        private static void AppPostStart()
        {
            GlobalFilters.Filters.Add(new ElmahHandleErrorAttribute());
            GlobalFilters.Filters.Add(new ReadOnlyModeErrorFilter());
            GlobalFilters.Filters.Add(new AntiForgeryErrorFilter());

            // disable the X-AspNetMvc-Version: header
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // set up MVC routes so our app URLs actually work
            // IMPORTANT: this must be called last; nothing else appears to execute after this
            RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Register our ASP.NET MVC routes
        /// </summary>
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("errors");
            routes.IgnoreRoute("errors/{*pathInfo}");
            //routes.IgnoreRoute("{*allaspx}", new {allaspx = @".*\.aspx(/.*)?"});
            routes.IgnoreRoute("{*allaxd}", new {allaxd = @".*\.axd(/.*)?"});
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("favicon.png");

            RouteAttribute.MapDecoratedRoutes(routes);

            // MUST be the last route as a catch-all!
            routes.MapRoute("{*url}", new { controller = "Error", action = "PageNotFound" }.ToString());
        }

        private static void NinjectPreStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            NinjectBootstrapper.Initialize(() => Container.Kernel);
        }

        private static void NinjectStop()
        {
            NinjectBootstrapper.ShutDown();
        }
    }
}
