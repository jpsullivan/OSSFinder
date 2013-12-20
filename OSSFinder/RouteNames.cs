using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSSFinder
{
    public static class RouteName
    {
        public const string Account = "Account";
        public const string Profile = "Profile";
        public const string Home = "Home";
        public const string Authentication = "SignIn";
        public const string UserAction = "UserAction";
        public const string PasswordReset = "PasswordReset";
        public const string PasswordSet = "PasswordSet";
        public const string ServiceAlert = "ServiceAlert";
        public const string OwinRoute = "OwinRoute";
        public const string ExternalAuthentication = "ExternalAuthentication";
        public const string ExternalAuthenticationCallback = "ExternalAuthenticationCallback";
        public const string RemoveCredential = "RemoveCredential";
        public const string RemovePassword = "RemovePassword";
        public const string ConfirmAccount = "ConfirmAccount";
        public const string SubscribeToEmails = "SubscribeToEmails";
        public const string UnsubscribeFromEmails = "UnsubscribeFromEmails";
        public const string Error500 = "Error500";
        public const string Error404 = "Error404";
    }
}