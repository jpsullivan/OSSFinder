using System;
using System.Data.Entity.Migrations;
using System.Linq;
using OSSFinder.Core.Entities;

namespace OSSFinder.Migrations
{
    public class MigrationsConfiguration : DbMigrationsConfiguration<EntitiesContext>
    {
        public MigrationsConfiguration()
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
            if (!gallerySettings.Any())
            {
                gallerySettings.Add(new SiteSetting { Key = 1 });
                context.SaveChanges();
            }
        }
    }
}
