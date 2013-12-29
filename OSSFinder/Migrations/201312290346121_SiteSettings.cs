namespace OSSFinder.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class SiteSettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SiteSettings",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SiteSettings");
        }
    }
}
