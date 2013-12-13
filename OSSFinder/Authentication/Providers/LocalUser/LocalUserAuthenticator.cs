using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using OSSFinder.Configuration;
using Owin;

namespace OSSFinder.Authentication.Providers.LocalUser
{
    public class LocalUserAuthenticator : Authenticator
    {
        protected override void AttachToOwinApp(ConfigurationService config, IAppBuilder app)
        {
            var cookieSecurity = config.Current.RequireSSL ?
                CookieSecureOption.Always :
                CookieSecureOption.Never;

            var options = new CookieAuthenticationOptions()
            {
                AuthenticationType = AuthenticationTypes.LocalUser,
                AuthenticationMode = AuthenticationMode.Active,
                CookieHttpOnly = true,
                CookieSecure = cookieSecurity,
                LoginPath = new PathString("/users/account/LogOn")
            };

            BaseConfig.ApplyToOwinSecurityOptions(options);
            app.UseCookieAuthentication(options);
            app.SetDefaultSignInAsAuthenticationType(AuthenticationTypes.LocalUser);
        }

        protected internal override AuthenticatorConfiguration CreateConfigObject()
        {
            return new AuthenticatorConfiguration()
            {
                AuthenticationType = AuthenticationTypes.LocalUser,
                Enabled = false
            };
        }
    }
}