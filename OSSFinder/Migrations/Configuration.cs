using System.Data.Entity.Migrations;
using System.Linq;
using OSSFinder.Entities;

namespace OSSFinder.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<EntitiesContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EntitiesContext context)
        {
            var roles = context.Set<Role>();
            if (!roles.Any(x => x.Name == Constants.AdminRoleName))
            {
                roles.Add(new Role { Name = Constants.AdminRoleName });
                context.SaveChanges();
            }

            var gallerySettings = context.Set<SiteSetting>();
            if (gallerySettings.Any()) return;

            gallerySettings.Add(new SiteSetting { Key = 1 });
            context.SaveChanges();
        }
    }
}
