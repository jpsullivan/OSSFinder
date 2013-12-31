using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using AnglicanGeek.MarkdownMailer;
using Elmah;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using OSSFinder.Auditing;
using OSSFinder.Configuration;
using OSSFinder.Entities;
using OSSFinder.Infrastructure;
using OSSFinder.Services;
using OSSFinder.Services.Interfaces;
using PoliteCaptcha;
using EntitiesContext = OSSFinder.Entities.EntitiesContext;
using IEntitiesContext = OSSFinder.Entities.IEntitiesContext;

namespace OSSFinder.App_Start
{
    public class ContainerBindings : NinjectModule
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:CyclomaticComplexity", Justification = "This code is more maintainable in the same function.")]
        public override void Load()
        {
            var configuration = new ConfigurationService();
            Bind<ConfigurationService>()
                .ToMethod(context => configuration);
            Bind<IAppConfiguration>()
                .ToMethod(context => configuration.Current);
            Bind<IConfigurationSource>()
                .ToMethod(context => configuration);

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

            Bind<ICacheService>()
                .To<HttpContextCacheService>()
                .InRequestScope();

            Bind<IContentService>()
                .To<ContentService>()
                .InSingletonScope();

            Bind<IEntitiesContext>()
                .ToMethod(context => new EntitiesContext(configuration.Current.SqlConnectionString, readOnly: configuration.Current.ReadOnlyMode))
                .InRequestScope();

            Bind<IEntityRepository<User>>()
                .To<EntityRepository<User>>()
                .InRequestScope();

            Bind<IEntityRepository<ProjectRegistration>>()
                .To<EntityRepository<ProjectRegistration>>()
                .InRequestScope();

            Bind<IEntityRepository<Project>>()
                .To<EntityRepository<Project>>()
                .InRequestScope();

            Bind<IEntityRepository<Credential>>()
                .To<EntityRepository<Credential>>()
                .InRequestScope();

            Bind<IUserService>()
                .To<UserService>()
                .InRequestScope();

            Bind<IProjectService>()
                .To<ProjectService>()
                .InRequestScope();

            Bind<IFormsAuthenticationService>()
                .To<FormsAuthenticationService>()
                .InSingletonScope();

            Bind<IControllerFactory>()
                .To<OssFinderControllerFactory>()
                .InRequestScope();

            var mailSenderThunk = new Lazy<IMailSender>(
                () =>
                {
                    var settings = Kernel.Get<ConfigurationService>();
                    if (settings.Current.SmtpUri != null && settings.Current.SmtpUri.IsAbsoluteUri)
                    {
                        var smtpUri = new SmtpUri(settings.Current.SmtpUri);

                        var mailSenderConfiguration = new MailSenderConfiguration
                        {
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Host = smtpUri.Host,
                            Port = smtpUri.Port,
                            EnableSsl = smtpUri.Secure
                        };

                        if (!String.IsNullOrWhiteSpace(smtpUri.UserName))
                        {
                            mailSenderConfiguration.UseDefaultCredentials = false;
                            mailSenderConfiguration.Credentials = new NetworkCredential(
                                smtpUri.UserName,
                                smtpUri.Password);
                        }

                        return new MailSender(mailSenderConfiguration);
                    }
                    else
                    {
                        var mailSenderConfiguration = new MailSenderConfiguration
                        {
                            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                            PickupDirectoryLocation = HostingEnvironment.MapPath("~/App_Data/Mail")
                        };

                        return new MailSender(mailSenderConfiguration);
                    }
                });

            Bind<IMailSender>()
                .ToMethod(context => mailSenderThunk.Value);

            Bind<IMessageService>()
                .To<MessageService>();

            Bind<IPrincipal>().ToMethod(context => HttpContext.Current.User);

            switch (configuration.Current.StorageType)
            {
                case StorageType.FileSystem:
                case StorageType.NotSpecified:
                    ConfigureForLocalFileSystem();
                    break;
                case StorageType.AzureStorage:
                    ConfigureForAzureStorage(configuration);
                    break;
            }

            Bind<IFileSystemService>()
                .To<FileSystemService>()
                .InSingletonScope();
        }

        private void ConfigureForLocalFileSystem()
        {
            Bind<IFileStorageService>()
                .To<FileSystemFileStorageService>()
                .InSingletonScope();

            // Ninject is doing some weird things with constructor selection without these.
            // Anyone requesting an IReportService or IStatisticsService should be prepared
            // to receive null anyway.
            Bind<IReportService>().ToConstant(NullReportService.Instance);
            Bind<IStatisticsService>().ToConstant(NullStatisticsService.Instance);
            Bind<AuditingService>().ToConstant(AuditingService.None);
        }

        private void ConfigureForAzureStorage(ConfigurationService configuration)
        {
            Bind<ICloudBlobClient>()
                .ToMethod(
                    _ => new CloudBlobClientWrapper(configuration.Current.AzureStorageConnectionString))
                .InSingletonScope();
            Bind<IFileStorageService>()
                .To<CloudBlobFileStorageService>()
                .InSingletonScope();

            // when running on Windows Azure, pull the statistics from the warehouse via storage
            Bind<IReportService>()
                .ToMethod(context => new CloudReportService(configuration.Current.AzureStorageConnectionString))
                .InSingletonScope();
//            Bind<IStatisticsService>()
//                .To<JsonStatisticsService>()
//                .InSingletonScope();

            string instanceId;
            try
            {
                instanceId = RoleEnvironment.CurrentRoleInstance.Id;
            }
            catch (Exception)
            {
                instanceId = Environment.MachineName;
            }

            var localIP = AuditActor.GetLocalIP().Result;

            Bind<AuditingService>()
                .ToMethod(_ => new CloudAuditingService(
                    instanceId, localIP, configuration.Current.AzureStorageConnectionString, CloudAuditingService.AspNetActorThunk))
                .InSingletonScope();
        }
    }
}