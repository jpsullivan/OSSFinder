namespace OSSFinder.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialProjectsAndProjectRegistrations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectRegistrations",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.PackageRegistrationOwners",
                c => new
                    {
                        PackageRegistrationKey = c.Int(nullable: false),
                        UserKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PackageRegistrationKey, t.UserKey })
                .ForeignKey("dbo.ProjectRegistrations", t => t.PackageRegistrationKey, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserKey, cascadeDelete: true)
                .Index(t => t.PackageRegistrationKey)
                .Index(t => t.UserKey);
            
            AddColumn("dbo.Projects", "ProjectRegistrationKey", c => c.Int(nullable: false));
            AddForeignKey("dbo.Projects", "ProjectRegistrationKey", "dbo.ProjectRegistrations", "Key", cascadeDelete: true);
            CreateIndex("dbo.Projects", "ProjectRegistrationKey");
            DropColumn("dbo.Projects", "HashAlgorithm");
            DropColumn("dbo.Projects", "Hash");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "Hash", c => c.String(nullable: false, maxLength: 256));
            AddColumn("dbo.Projects", "HashAlgorithm", c => c.String(maxLength: 10));
            DropIndex("dbo.PackageRegistrationOwners", new[] { "UserKey" });
            DropIndex("dbo.PackageRegistrationOwners", new[] { "PackageRegistrationKey" });
            DropIndex("dbo.Projects", new[] { "ProjectRegistrationKey" });
            DropForeignKey("dbo.PackageRegistrationOwners", "UserKey", "dbo.Users");
            DropForeignKey("dbo.PackageRegistrationOwners", "PackageRegistrationKey", "dbo.ProjectRegistrations");
            DropForeignKey("dbo.Projects", "ProjectRegistrationKey", "dbo.ProjectRegistrations");
            DropColumn("dbo.Projects", "ProjectRegistrationKey");
            DropTable("dbo.PackageRegistrationOwners");
            DropTable("dbo.ProjectRegistrations");
        }
    }
}
