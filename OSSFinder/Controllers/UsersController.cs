﻿using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using OSSFinder.Authentication;
using OSSFinder.Configuration;
using OSSFinder.Entities;
using OSSFinder.Infrastructure.Attributes;
using OSSFinder.Infrastructure.Extensions;
using OSSFinder.Models.ViewModels;
using OSSFinder.Services.Interfaces;
using CredentialTypes = OSSFinder.Authentication.CredentialTypes;

namespace OSSFinder.Controllers
{
    public partial class UsersController : AppController
    {
        public IUserService UserService { get; protected set; }
        public IMessageService MessageService { get; protected set; }
        public IAppConfiguration Config { get; protected set; }
        public AuthenticationService AuthService { get; protected set; }

        public UsersController(
            IUserService userService,
            IMessageService messageService,
            IAppConfiguration config,
            AuthenticationService authService)
        {
            UserService = userService;
            MessageService = messageService;
            Config = config;
            AuthService = authService;
        }

        [HttpGet]
        [Authorize]
        public virtual ActionResult ConfirmationRequired()
        {
            User user = GetCurrentUser();
            var model = new ConfirmationViewModel
            {
                ConfirmingNewAccount = !(user.Confirmed),
                UnconfirmedEmailAddress = user.UnconfirmedEmailAddress,
            };
            return View(model);
        }

        [Authorize]
        [ActionName("ConfirmationRequired"), AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult ConfirmationRequiredPost()
        {
            User user = GetCurrentUser();
            var confirmationUrl = Url.ConfirmationUrl(
                "Confirm", "Users", user.Username, user.EmailConfirmationToken);

            MessageService.SendNewAccountEmail(new MailAddress(user.UnconfirmedEmailAddress, user.Username), confirmationUrl);

            var model = new ConfirmationViewModel
            {
                ConfirmingNewAccount = !(user.Confirmed),
                UnconfirmedEmailAddress = user.UnconfirmedEmailAddress,
                SentEmail = true,
            };
            return View(model);
        }

        [Authorize]
        public virtual ActionResult Account()
        {
            return AccountView(new AccountViewModel());
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/subscribe"), AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult ChangeEmailSubscription(bool subscribe)
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return HttpNotFound();
            }

            UserService.ChangeEmailSubscription(user, subscribe);
            TempData["Message"] = Strings.EmailPreferencesUpdated;
            return RedirectToAction("Account");
        }

        public virtual ActionResult Thanks()
        {
            // No need to redirect here after someone logs in...
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;
            return View();
        }

        public virtual ActionResult ForgotPassword()
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            if (ModelState.IsValid)
            {
                var user = await AuthService.GeneratePasswordResetToken(model.Email, Constants.DefaultPasswordResetTokenExpirationHours * 60);
                if (user != null)
                {
                    return SendPasswordResetEmail(user, forgotPassword: true);
                }

                ModelState.AddModelError("Email", "Could not find anyone with that email.");
            }

            return View(model);
        }

        public virtual ActionResult PasswordSent()
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            ViewBag.Email = TempData["Email"];
            ViewBag.Expiration = Constants.DefaultPasswordResetTokenExpirationHours;
            return View();
        }

        public virtual ActionResult ResetPassword(bool forgot)
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            ViewBag.ResetTokenValid = true;
            ViewBag.ForgotPassword = forgot;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/forgotpassword/{username}/{token}")]
        public virtual async Task<ActionResult> ResetPassword(string username, string token, PasswordResetViewModel model, bool forgot)
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            var cred = await AuthService.ResetPasswordWithToken(username, token, model.NewPassword);
            ViewBag.ResetTokenValid = cred != null;
            ViewBag.ForgotPassword = forgot;

            if (!ViewBag.ResetTokenValid)
            {
                ModelState.AddModelError("", "The Password Reset Token is not valid or expired.");
                return View(model);
            }

            if (cred != null && !forgot)
            {
                // Setting a password, so notify the user
                MessageService.SendCredentialAddedNotice(cred.User, cred);
            }

            return RedirectToAction("PasswordChanged");
        }

        [Authorize]
        [Route("account/confirm/{username}/{token}")]
        public virtual async Task<ActionResult> Confirm(string username, string token)
        {
            // We don't want Login to have us as a return URL
            // By having this value present in the dictionary BUT null, we don't put "returnUrl" on the Login link at all
            ViewData[Constants.ReturnUrlViewDataKey] = null;

            if (!String.Equals(username, User.Identity.Name, StringComparison.OrdinalIgnoreCase))
            {
                return View(new ConfirmationViewModel
                {
                    WrongUsername = true,
                    SuccessfulConfirmation = false,
                });
            }

            var user = GetCurrentUser();

            string existingEmail = user.EmailAddress;
            var model = new ConfirmationViewModel
            {
                ConfirmingNewAccount = String.IsNullOrEmpty(existingEmail),
                SuccessfulConfirmation = true,
            };

            try
            {
                if (!(await UserService.ConfirmEmailAddress(user, token)))
                {
                    model.SuccessfulConfirmation = false;
                }
            }
            catch (EntityException)
            {
                model.SuccessfulConfirmation = false;
                model.DuplicateEmailAddress = true;
            }

            // SuccessfulConfirmation is required so that the confirm Action isn't a way to spam people.
            // Change notice not required for new accounts.
            if (model.SuccessfulConfirmation && !model.ConfirmingNewAccount)
            {
                MessageService.SendEmailChangeNoticeToPreviousEmailAddress(user, existingEmail);

                string returnUrl = HttpContext.GetConfirmationReturnUrl();
                if (!String.IsNullOrEmpty(returnUrl))
                {
                    TempData["Message"] = "You have successfully confirmed your email address!";
                    return SafeRedirect(returnUrl);
                }
            }

            return View(model);
        }

        public virtual ActionResult Profiles(string username)
        {
            var user = UserService.FindByUsername(username);
            if (user == null)
            {
                return HttpNotFound();
            }

//            var packages = PackageService.FindPackagesByOwner(user, includeUnlisted: false)
//                .Select(p => new PackageViewModel(p)
//                {
//                    DownloadCount = p.PackageRegistration.DownloadCount,
//                    Version = null
//                }).ToList();

            var model = new UserProfileModel(user);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> ChangeEmail(AccountViewModel model)
        {
            if (!ModelState.IsValidField("ChangeEmail.NewEmail"))
            {
                return AccountView(model);
            }

            var user = GetCurrentUser();
            if (user.HasPassword())
            {
                if (!ModelState.IsValidField("ChangeEmail.Password"))
                {
                    return AccountView(model);
                }

                var authUser = await AuthService.Authenticate(User.Identity.Name, model.ChangeEmail.Password);
                if (authUser == null)
                {
                    ModelState.AddModelError("ChangeEmail.Password", Strings.CurrentPasswordIncorrect);
                    return AccountView(model);
                }
            }
            // No password? We can't do any additional verification...

            if (String.Equals(model.ChangeEmail.NewEmail, user.LastSavedEmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                // email address unchanged - accept
                return RedirectToAction("Account");
            }

            try
            {
                await UserService.ChangeEmailAddress(user, model.ChangeEmail.NewEmail);
            }
            catch (EntityException e)
            {
                ModelState.AddModelError("NewEmail", e.Message);
                return AccountView(model);
            }

            if (user.Confirmed)
            {
                var confirmationUrl = Url.ConfirmationUrl(
                    "Confirm", "Users", user.Username, user.EmailConfirmationToken);
                MessageService.SendEmailChangeConfirmationNotice(new MailAddress(user.UnconfirmedEmailAddress, user.Username), confirmationUrl);

                TempData["Message"] = Strings.EmailUpdated_ConfirmationRequired;
            }
            else
            {
                TempData["Message"] = Strings.EmailUpdated;
            }

            return RedirectToAction("Account");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ChangePassword(AccountViewModel model)
        {
            var user = GetCurrentUser();

            var oldPassword = user.Credentials.FirstOrDefault(
                c => c.Type.StartsWith(CredentialTypes.Password.Prefix, StringComparison.OrdinalIgnoreCase));

            if (oldPassword == null)
            {
                // User is requesting a password set email
                await AuthService.GeneratePasswordResetToken(user, Constants.DefaultPasswordResetTokenExpirationHours * 60);
                return SendPasswordResetEmail(user, forgotPassword: false);
            }
            else
            {
                if (!ModelState.IsValidField("ChangePassword"))
                {
                    return AccountView(model);
                }

                if (!(await AuthService.ChangePassword(user, model.ChangePassword.OldPassword, model.ChangePassword.NewPassword)))
                {
                    ModelState.AddModelError("ChangePassword.OldPassword", Strings.CurrentPasswordIncorrect);
                    return AccountView(model);
                }

                TempData["Message"] = Strings.PasswordChanged;

                return RedirectToAction("Account");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/RemoveCredential/password")]
        public virtual Task<ActionResult> RemovePassword()
        {
            var user = GetCurrentUser();
            var passwordCred = user.Credentials.SingleOrDefault(
                c => c.Type.StartsWith(CredentialTypes.Password.Prefix, StringComparison.OrdinalIgnoreCase));

            return RemoveCredential(user, passwordCred, Strings.PasswordRemoved);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/RemoveCredential/{credentialType}")]
        public virtual Task<ActionResult> RemoveCredential(string credentialType)
        {
            var user = GetCurrentUser();
            var cred = user.Credentials.SingleOrDefault(
                c => String.Equals(c.Type, credentialType, StringComparison.OrdinalIgnoreCase));

            return RemoveCredential(user, cred, Strings.CredentialRemoved);
        }

        public virtual ActionResult PasswordChanged()
        {
            return View();
        }

        private async Task<ActionResult> RemoveCredential(User user, Credential cred, string message)
        {
            // Count login credentials
            if (CountLoginCredentials(user) <= 1)
            {
                TempData["Message"] = Strings.CannotRemoveOnlyLoginCredential;
            }
            else if (cred != null)
            {
                await AuthService.RemoveCredential(user, cred);

                // Notify the user of the change
                MessageService.SendCredentialRemovedNotice(user, cred);

                TempData["Message"] = message;
            }
            return RedirectToAction("Account");
        }

        private ActionResult EditProfileView()
        {
            return AccountView(new AccountViewModel());
        }

        private ActionResult AccountView(AccountViewModel model)
        {
            // Load Credential info
            var user = GetCurrentUser();
            var creds = user.Credentials.Select(c => AuthService.DescribeCredential(c)).ToList();

            model.Credentials = creds;
            return View("Account", model);
        }

        private static int CountLoginCredentials(User user)
        {
            return user.Credentials.Count(c =>
                c.Type.StartsWith(CredentialTypes.Password.Prefix, StringComparison.OrdinalIgnoreCase) ||
                c.Type.StartsWith(CredentialTypes.ExternalPrefix, StringComparison.OrdinalIgnoreCase));
        }

        private ActionResult SendPasswordResetEmail(User user, bool forgotPassword)
        {
            var resetPasswordUrl = Url.ConfirmationUrl(
                "ResetPassword",
                "Users",
                user.Username,
                user.PasswordResetToken,
                new { forgot = forgotPassword });
            MessageService.SendPasswordResetInstructions(user, resetPasswordUrl, forgotPassword);

            TempData["Email"] = user.EmailAddress;
            return RedirectToAction("PasswordSent");
        }
    }
}
