namespace OSSFinder.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ProjectDependencies : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectDependencies",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        ProjectKey = c.Int(nullable: false),
                        DependencyKey = c.Int(nullable: false),
                        DependencyVersionKey = c.Int(nullable: false),
                        Language = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Projects", t => t.ProjectKey, cascadeDelete: true)
                .Index(t => t.ProjectKey);
            
            DropColumn("dbo.Projects", "ReleaseNotes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Projects", "ReleaseNotes", c => c.String());
            DropIndex("dbo.ProjectDependencies", new[] { "ProjectKey" });
            DropForeignKey("dbo.ProjectDependencies", "ProjectKey", "dbo.Projects");
            DropTable("dbo.ProjectDependencies");
        }
    }
}
