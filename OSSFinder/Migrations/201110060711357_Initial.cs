using System.Data.Entity.Migrations;

namespace OSSFinder.Migrations
{
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Users",
                c => new
                {
                    Key = c.Int(nullable: false, identity: true),
                    ApiKey = c.Guid(nullable: false),
                    EmailAddress = c.String(),
                    UnconfirmedEmailAddress = c.String(),
                    HashedPassword = c.String(),
                    Username = c.String(),
                    EmailAllowed = c.Boolean(nullable: false),
                    EmailConfirmationToken = c.String(),
                    PasswordResetToken = c.String(),
                    PasswordResetTokenExpirationDate = c.DateTime(),
                })
                .PrimaryKey(t => t.Key);

            CreateTable(
                "EmailMessages",
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
                .ForeignKey("Users", t => t.FromUserKey)
                .ForeignKey("Users", t => t.ToUserKey);

            CreateTable(
                "Roles",
                c => new
                {
                    Key = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Key);

            CreateTable(
                "UserRoles",
                c => new
                {
                    UserKey = c.Int(nullable: false),
                    RoleKey = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.UserKey, t.RoleKey })
                .ForeignKey("Users", t => t.UserKey)
                .ForeignKey("Roles", t => t.RoleKey);
        }

        public override void Down()
        {
            DropForeignKey("UserRoles", "RoleKey", "Roles", "Key");
            DropForeignKey("UserRoles", "UserKey", "Users", "Key");
            DropForeignKey("EmailMessages", "ToUserKey", "Users", "Key");
            DropForeignKey("EmailMessages", "FromUserKey", "Users", "Key");
            DropTable("UserRoles");
            DropTable("Roles");
            DropTable("EmailMessages");
            DropTable("Users");
        }
    }
}