namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fbNERs_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            /*MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].ClientKeys", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].RegisterApps", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_ARABICDARIJAENTRY_LATINWORD", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_ARABICDARIJAENTRY_TEXTENTITY", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_ARABICDARIJAENTRY", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_ARABIZIENTRY", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_TWINGLYACCOUNT", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_XTRCTTHEME_KEYWORD", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].T_XTRCTTHEME", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].RegisterAppCallingLogs", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].TokensManagers", newSchema: "dbo");
            MoveTable(name: "[ArabicTextAnalyzer.Models.ArabiziDbContext].RegisterUser", newSchema: "dbo");*/
            AddColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "FK_ENTRY", c => c.String(maxLength: 150, unicode: false));
            AddColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "ENTRY_type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "ENTRY_type");
            DropColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "FK_ENTRY");
            /*MoveTable(name: "dbo.RegisterUser", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.TokensManagers", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.RegisterAppCallingLogs", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_XTRCTTHEME", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_XTRCTTHEME_KEYWORD", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_TWINGLYACCOUNT", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_ARABIZIENTRY", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_ARABICDARIJAENTRY", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_ARABICDARIJAENTRY_TEXTENTITY", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.T_ARABICDARIJAENTRY_LATINWORD", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.RegisterApps", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");
            MoveTable(name: "dbo.ClientKeys", newSchema: "ArabicTextAnalyzer.Models.ArabiziDbContext");*/
        }
    }
}
