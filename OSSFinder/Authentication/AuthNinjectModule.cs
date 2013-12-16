using Ninject.Modules;
using OSSFinder.Authentication.Providers;

namespace OSSFinder.Authentication
{
    public class AuthNinjectModule : NinjectModule
    {
        public override void Load()
        {
            foreach (var instance in Authenticator.GetAllAvailable())
            {
                Bind(typeof(Authenticator))
                    .ToConstant(instance)
                    .InSingletonScope();
            }
        }
    }
}