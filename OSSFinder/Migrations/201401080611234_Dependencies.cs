namespace OSSFinder.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dependencies : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PackageRegistrationOwners", newName: "ProjectRegistrationOwners");
            RenameColumn(table: "dbo.ProjectRegistrationOwners", name: "PackageRegistrationKey", newName: "ProjectRegistrationKey");
            CreateTable(
                "dbo.Dependencies",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        RepositoryUrl = c.String(),
                        PackageUrl = c.String(),
                        Created = c.DateTime(nullable: false),
                        LastModified = c.DateTime(nullable: false),
                        Type_Key = c.Int(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.DependencyTypes", t => t.Type_Key)
                .Index(t => t.Type_Key);
            
            CreateTable(
                "dbo.DependencyTypes",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Enabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.DependencyVersions",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        DependencyKey = c.Int(nullable: false),
                        Version = c.String(nullable: false, maxLength: 64),
                        Language = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Dependencies", t => t.DependencyKey, cascadeDelete: true)
                .Index(t => t.DependencyKey);
            
            AddForeignKey("dbo.ProjectDependencies", "DependencyKey", "dbo.Dependencies", "Key", cascadeDelete: true);
            AddForeignKey("dbo.ProjectDependencies", "DependencyVersionKey", "dbo.DependencyVersions", "Key", cascadeDelete: true);
            CreateIndex("dbo.ProjectDependencies", "DependencyKey");
            CreateIndex("dbo.ProjectDependencies", "DependencyVersionKey");
            DropColumn("dbo.ProjectDependencies", "Language");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProjectDependencies", "Language", c => c.Int(nullable: false));
            DropIndex("dbo.ProjectDependencies", new[] { "DependencyVersionKey" });
            DropIndex("dbo.ProjectDependencies", new[] { "DependencyKey" });
            DropIndex("dbo.DependencyVersions", new[] { "DependencyKey" });
            DropIndex("dbo.Dependencies", new[] { "Type_Key" });
            DropForeignKey("dbo.ProjectDependencies", "DependencyVersionKey", "dbo.DependencyVersions");
            DropForeignKey("dbo.ProjectDependencies", "DependencyKey", "dbo.Dependencies");
            DropForeignKey("dbo.DependencyVersions", "DependencyKey", "dbo.Dependencies");
            DropForeignKey("dbo.Dependencies", "Type_Key", "dbo.DependencyTypes");
            DropTable("dbo.DependencyVersions");
            DropTable("dbo.DependencyTypes");
            DropTable("dbo.Dependencies");
            RenameColumn(table: "dbo.ProjectRegistrationOwners", name: "ProjectRegistrationKey", newName: "PackageRegistrationKey");
            RenameTable(name: "dbo.ProjectRegistrationOwners", newName: "PackageRegistrationOwners");
        }
    }
}
