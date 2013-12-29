namespace OSSFinder.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Credentials",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        UserKey = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 64),
                        Value = c.String(nullable: false, maxLength: 256),
                        Identity = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Users", t => t.UserKey, cascadeDelete: true)
                .Index(t => t.UserKey);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        EmailAddress = c.String(maxLength: 256),
                        UnconfirmedEmailAddress = c.String(maxLength: 256),
                        Username = c.String(nullable: false, maxLength: 64),
                        EmailAllowed = c.Boolean(nullable: false),
                        EmailConfirmationToken = c.String(maxLength: 32),
                        PasswordResetToken = c.String(maxLength: 32),
                        PasswordResetTokenExpirationDate = c.DateTime(),
                        CreatedUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.EmailMessages",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                        FromUserKey = c.Int(),
                        Sent = c.Boolean(nullable: false),
                        Subject = c.String(),
                        ToUserKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Users", t => t.FromUserKey)
                .ForeignKey("dbo.Users", t => t.ToUserKey, cascadeDelete: true)
                .Index(t => t.FromUserKey)
                .Index(t => t.ToUserKey);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserKey = c.Int(nullable: false),
                        RoleKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserKey, t.RoleKey })
                .ForeignKey("dbo.Users", t => t.UserKey, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleKey, cascadeDelete: true)
                .Index(t => t.UserKey)
                .Index(t => t.RoleKey);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserRoles", new[] { "RoleKey" });
            DropIndex("dbo.UserRoles", new[] { "UserKey" });
            DropIndex("dbo.EmailMessages", new[] { "ToUserKey" });
            DropIndex("dbo.EmailMessages", new[] { "FromUserKey" });
            DropIndex("dbo.Credentials", new[] { "UserKey" });
            DropForeignKey("dbo.UserRoles", "RoleKey", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "UserKey", "dbo.Users");
            DropForeignKey("dbo.EmailMessages", "ToUserKey", "dbo.Users");
            DropForeignKey("dbo.EmailMessages", "FromUserKey", "dbo.Users");
            DropForeignKey("dbo.Credentials", "UserKey", "dbo.Users");
            DropTable("dbo.UserRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.EmailMessages");
            DropTable("dbo.Users");
            DropTable("dbo.Credentials");
        }
    }
}
