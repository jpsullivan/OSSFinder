using System.Security.Principal;

namespace OSSFinder.Infrastructure.Extensions
{
    public static class PrincipalExtensions
    {
        public static bool IsAdministrator(this IPrincipal self)
        {
            return self.IsInRole(Constants.AdminRoleName);
        }
    }
}