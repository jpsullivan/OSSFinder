using System;
using System.Globalization;
using System.Net.Mail;
using System.Web;
using AnglicanGeek.MarkdownMailer;
using Elmah;
using OSSFinder.Authentication;
using OSSFinder.Configuration;
using OSSFinder.Entities;
using OSSFinder.Infrastructure.Extensions;
using OSSFinder.Services.Interfaces;

namespace OSSFinder.Services
{
    public class MessageService : IMessageService
    {
        public IMailSender MailSender { get; protected set; }
        public IAppConfiguration Config { get; protected set; }
        public AuthenticationService AuthService { get; protected set; }

        protected MessageService() { }

        public MessageService(IMailSender mailSender, IAppConfiguration config, AuthenticationService authService)
            : this()
        {
            MailSender = mailSender;
            Config = config;
            AuthService = authService;
        }

        public void SendNewAccountEmail(MailAddress toAddress, string confirmationUrl)
        {
            string body = @"Thank you for registering with the {0}. 
We can't wait to see what packages you'll upload.

So we can be sure to contact you, please verify your email address and click the following link:

[{1}]({2})

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                Config.GalleryOwner.DisplayName,
                HttpUtility.UrlDecode(confirmationUrl),
                confirmationUrl);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(CultureInfo.CurrentCulture, "[{0}] Please verify your account.", Config.GalleryOwner.DisplayName);
                mailMessage.Body = body;
                mailMessage.From = Config.GalleryOwner;

                mailMessage.To.Add(toAddress);
                SendMessage(mailMessage);
            }
        }

        public void SendEmailChangeConfirmationNotice(MailAddress newEmailAddress, string confirmationUrl)
        {
            string body = @"You recently changed your {0} email address. 

To verify your new email address, please click the following link:

[{1}]({2})

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                Config.GalleryOwner.DisplayName,
                HttpUtility.UrlDecode(confirmationUrl),
                confirmationUrl);

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = String.Format(
                    CultureInfo.CurrentCulture, "[{0}] Please verify your new email address.", Config.GalleryOwner.DisplayName);
                mailMessage.Body = body;
                mailMessage.From = Config.GalleryOwner;

                mailMessage.To.Add(newEmailAddress);
                SendMessage(mailMessage);
            }
        }

        public void SendEmailChangeNoticeToPreviousEmailAddress(User user, string oldEmailAddress)
        {
            string body = @"Hi there,

The email address associated to your {0} account was recently 
changed from _{1}_ to _{2}_.

Thanks,
The {0} Team";

            body = String.Format(
                CultureInfo.CurrentCulture,
                body,
                Config.GalleryOwner.DisplayName,
                oldEmailAddress,
                user.EmailAddress);

            string subject = String.Format(CultureInfo.CurrentCulture, "[{0}] Recent changes to your account.", Config.GalleryOwner.DisplayName);
            using (
                var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = Config.GalleryOwner;

                mailMessage.To.Add(new MailAddress(oldEmailAddress, user.Username));
                SendMessage(mailMessage);
            }
        }

        public void SendPasswordResetInstructions(User user, string resetPasswordUrl, bool forgotPassword)
        {
            string body = String.Format(
                CultureInfo.CurrentCulture,
                forgotPassword ? Strings.Emails_ForgotPassword_Body : Strings.Emails_SetPassword_Body,
                Constants.DefaultPasswordResetTokenExpirationHours,
                resetPasswordUrl,
                Config.GalleryOwner.DisplayName);

            string subject = String.Format(CultureInfo.CurrentCulture, forgotPassword ? Strings.Emails_ForgotPassword_Subject : Strings.Emails_SetPassword_Subject, Config.GalleryOwner.DisplayName);
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = Config.GalleryOwner;

                mailMessage.To.Add(user.ToMailAddress());
                SendMessage(mailMessage);
            }
        }

        public void SendCredentialRemovedNotice(User user, Credential removed)
        {
            SendCredentialChangeNotice(
                user,
                removed,
                Strings.Emails_CredentialRemoved_Body,
                Strings.Emails_CredentialRemoved_Subject);
        }

        public void SendCredentialAddedNotice(User user, Credential added)
        {
            SendCredentialChangeNotice(
                user,
                added,
                Strings.Emails_CredentialAdded_Body,
                Strings.Emails_CredentialAdded_Subject);
        }

        private void SendCredentialChangeNotice(User user, Credential changed, string bodyTemplate, string subjectTemplate)
        {
            // What kind of credential is this?
            var credViewModel = AuthService.DescribeCredential(changed);
            string name = credViewModel.AuthUI == null ? credViewModel.TypeCaption : credViewModel.AuthUI.AccountNoun;

            string body = String.Format(
                CultureInfo.CurrentCulture,
                bodyTemplate,
                name);
            string subject = String.Format(
                CultureInfo.CurrentCulture,
                subjectTemplate,
                Config.GalleryOwner.DisplayName,
                name);
            SendSupportMessage(user, body, subject);
        }

        private void SendSupportMessage(User user, string body, string subject)
        {
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.From = Config.GalleryOwner;

                mailMessage.To.Add(user.ToMailAddress());
                SendMessage(mailMessage);
            }
        }

        private void SendMessage(MailMessage mailMessage)
        {
            try
            {
                MailSender.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Log but swallow the exception
                ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
    }
}