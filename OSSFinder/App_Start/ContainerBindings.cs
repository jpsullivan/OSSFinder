using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Elmah;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using OSSFinder.Configuration;
using PoliteCaptcha;

namespace OSSFinder.App_Start
{
    public class ContainerBindings : NinjectModule
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:CyclomaticComplexity", Justification = "This code is more maintainable in the same function.")]
        public override void Load()
        {
            // Used for suppressing any IntPtr fatal exceptions
            Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            var configuration = new ConfigurationService();
            Bind<ConfigurationService>().ToMethod(context => configuration);
            Bind<IAppConfiguration>().ToMethod(context => configuration.Current);
            Bind<IConfigurationSource>().ToMethod(context => configuration);

            if (!String.IsNullOrEmpty(configuration.Current.AzureStorageConnectionString))
            {
                Bind<ErrorLog>()
                    .ToMethod(_ => new TableErrorLog(configuration.Current.AzureStorageConnectionString))
                    .InSingletonScope();
            }
            else
            {
                Bind<ErrorLog>()
                    .ToMethod(_ => new SqlErrorLog(configuration.Current.SqlConnectionString))
                    .InSingletonScope();
            }
        }
    }
}