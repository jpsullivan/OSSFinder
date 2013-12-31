namespace OSSFinder.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddProjectsAndAuthors : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        Description = c.String(),
                        ReleaseNotes = c.String(),
                        DownloadCount = c.Int(nullable: false),
                        ExternalPackageUrl = c.String(),
                        HashAlgorithm = c.String(maxLength: 10),
                        Hash = c.String(nullable: false, maxLength: 256),
                        IconUrl = c.String(),
                        IsLatest = c.Boolean(nullable: false),
                        IsLatestStable = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        LastEdited = c.DateTime(),
                        LicenseUrl = c.String(),
                        Listed = c.Boolean(nullable: false),
                        HideLicenseReport = c.Boolean(nullable: false),
                        Language = c.String(maxLength: 20),
                        Published = c.DateTime(nullable: false),
                        ProjectUrl = c.String(),
                        RequiresLicenseAcceptance = c.Boolean(nullable: false),
                        Summary = c.String(),
                        Tags = c.String(),
                        Title = c.String(maxLength: 256),
                        FlattenedAuthors = c.String(),
                        UserKey = c.Int(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Users", t => t.UserKey)
                .Index(t => t.UserKey);
            
            CreateTable(
                "dbo.ProjectAuthors",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ProjectKey = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Projects", t => t.ProjectKey, cascadeDelete: true)
                .Index(t => t.ProjectKey);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProjectAuthors", new[] { "ProjectKey" });
            DropIndex("dbo.Projects", new[] { "UserKey" });
            DropForeignKey("dbo.ProjectAuthors", "ProjectKey", "dbo.Projects");
            DropForeignKey("dbo.Projects", "UserKey", "dbo.Users");
            DropTable("dbo.ProjectAuthors");
            DropTable("dbo.Projects");
        }
    }
}
