namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenAuthentication : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientKeys",
                c => new
                    {
                        ClientKeysID = c.Int(nullable: false, identity: true),
                        RegisterAppId = c.Int(),
                        ClientId = c.String(),
                        ClientSecret = c.String(),
                        CreatedOn = c.DateTime(),
                        UserID = c.String(),
                    })
                .PrimaryKey(t => t.ClientKeysID)
                .ForeignKey("dbo.RegisterApps", t => t.RegisterAppId)
                .Index(t => t.RegisterAppId);
            
            CreateTable(
                "dbo.RegisterApps",
                c => new
                    {
                        RegisterAppId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        TotalAppCallLimit = c.Int(nullable: false),
                        TotalAppCallConsumed = c.Int(nullable: false),
                        CreatedOn = c.DateTime(),
                        UserID = c.String(),
                    })
                .PrimaryKey(t => t.RegisterAppId);
            
            CreateTable(
                "dbo.RegisterAppCallingLogs",
                c => new
                    {
                        RegisterAppCallingLogId = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        RegisterAppId = c.Int(),
                        TokensManagerID = c.Int(),
                        DateCreatedOn = c.DateTime(),
                        MethodName = c.String(),
                    })
                .PrimaryKey(t => t.RegisterAppCallingLogId)
                .ForeignKey("dbo.RegisterApps", t => t.RegisterAppId)
                .ForeignKey("dbo.TokensManagers", t => t.TokensManagerID)
                .Index(t => t.RegisterAppId)
                .Index(t => t.TokensManagerID);
            
            CreateTable(
                "dbo.TokensManagers",
                c => new
                    {
                        TokensManagerID = c.Int(nullable: false, identity: true),
                        TokenKey = c.String(),
                        IssuedOn = c.DateTime(),
                        ExpiresOn = c.DateTime(),
                        CreaatedOn = c.DateTime(),
                        RegisterAppId = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TokensManagerID)
                .ForeignKey("dbo.RegisterApps", t => t.RegisterAppId)
                .Index(t => t.RegisterAppId);
            
            CreateTable(
                "dbo.RegisterUser",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 30),
                        Password = c.String(nullable: false, maxLength: 30),
                        CreateOn = c.DateTime(nullable: false),
                        EmailID = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegisterAppCallingLogs", "TokensManagerID", "dbo.TokensManagers");
            DropForeignKey("dbo.TokensManagers", "RegisterAppId", "dbo.RegisterApps");
            DropForeignKey("dbo.RegisterAppCallingLogs", "RegisterAppId", "dbo.RegisterApps");
            DropForeignKey("dbo.ClientKeys", "RegisterAppId", "dbo.RegisterApps");
            DropIndex("dbo.TokensManagers", new[] { "RegisterAppId" });
            DropIndex("dbo.RegisterAppCallingLogs", new[] { "TokensManagerID" });
            DropIndex("dbo.RegisterAppCallingLogs", new[] { "RegisterAppId" });
            DropIndex("dbo.ClientKeys", new[] { "RegisterAppId" });
            DropTable("dbo.RegisterUser");
            DropTable("dbo.TokensManagers");
            DropTable("dbo.RegisterAppCallingLogs");
            DropTable("dbo.RegisterApps");
            DropTable("dbo.ClientKeys");
        }
    }
}
