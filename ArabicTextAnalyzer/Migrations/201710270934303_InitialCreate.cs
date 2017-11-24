namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.T_ARABICDARIJAENTRY_LATINWORD",
                c => new
                    {
                        ID_ARABICDARIJAENTRY_LATINWORD = c.Guid(nullable: false),
                        ID_ARABICDARIJAENTRY = c.Guid(nullable: false),
                        LatinWord = c.String(),
                        VariantsCount = c.Int(nullable: false),
                        MostPopularVariant = c.String(),
                        Translation = c.String(),
                    })
                .PrimaryKey(t => t.ID_ARABICDARIJAENTRY_LATINWORD);
            
            CreateTable(
                "dbo.T_ARABICDARIJAENTRY_TEXTENTITY",
                c => new
                    {
                        ID_ARABICDARIJAENTRY_TEXTENTITY = c.Guid(nullable: false),
                        ID_ARABICDARIJAENTRY = c.Guid(nullable: false),
                        TextEntity_Count = c.Long(nullable: false),
                        TextEntity_EntityId = c.String(),
                        TextEntity_Mention = c.String(),
                        TextEntity_Normalized = c.String(),
                        TextEntity_Type = c.String(),
                    })
                .PrimaryKey(t => t.ID_ARABICDARIJAENTRY_TEXTENTITY);
            
            CreateTable(
                "dbo.T_ARABICDARIJAENTRY",
                c => new
                    {
                        ID_ARABICDARIJAENTRY = c.Guid(nullable: false),
                        ID_ARABIZIENTRY = c.Guid(nullable: false),
                        ArabicDarijaText = c.String(),
                    })
                .PrimaryKey(t => t.ID_ARABICDARIJAENTRY);
            
            CreateTable(
                "dbo.T_ARABIZIENTRY",
                c => new
                    {
                        ID_ARABIZIENTRY = c.Guid(nullable: false),
                        ArabiziText = c.String(),
                        ArabiziEntryDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID_ARABIZIENTRY);
            
            CreateTable(
                "dbo.T_TWINGLYACCOUNT",
                c => new
                    {
                        ID_TWINGLYACCOUNT_API_KEY = c.Guid(nullable: false),
                        UserName = c.String(),
                        calls_free = c.Int(nullable: false),
                        CurrentActive = c.String(),
                    })
                .PrimaryKey(t => t.ID_TWINGLYACCOUNT_API_KEY);
            
            CreateTable(
                "dbo.T_XTRCTTHEME_KEYWORD",
                c => new
                    {
                        ID_XTRCTTHEME_KEYWORD = c.Guid(nullable: false),
                        ID_XTRCTTHEME = c.Guid(nullable: false),
                        Keyword = c.String(),
                    })
                .PrimaryKey(t => t.ID_XTRCTTHEME_KEYWORD);
            
            CreateTable(
                "dbo.T_XTRCTTHEME",
                c => new
                    {
                        ID_XTRCTTHEME = c.Guid(nullable: false),
                        ThemeName = c.String(),
                        CurrentActive = c.String(),
                    })
                .PrimaryKey(t => t.ID_XTRCTTHEME);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.T_XTRCTTHEME");
            DropTable("dbo.T_XTRCTTHEME_KEYWORD");
            DropTable("dbo.T_TWINGLYACCOUNT");
            DropTable("dbo.T_ARABIZIENTRY");
            DropTable("dbo.T_ARABICDARIJAENTRY");
            DropTable("dbo.T_ARABICDARIJAENTRY_TEXTENTITY");
            DropTable("dbo.T_ARABICDARIJAENTRY_LATINWORD");
        }
    }
}
