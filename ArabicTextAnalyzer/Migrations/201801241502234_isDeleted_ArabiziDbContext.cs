namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isDeleted_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_ARABICDARIJAENTRY_LATINWORD", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.T_ARABICDARIJAENTRY", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.T_ARABIZIENTRY", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.T_XTRCTTHEME_KEYWORD", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.T_XTRCTTHEME", "IsDeleted", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_XTRCTTHEME", "IsDeleted");
            DropColumn("dbo.T_XTRCTTHEME_KEYWORD", "IsDeleted");
            DropColumn("dbo.T_ARABIZIENTRY", "IsDeleted");
            DropColumn("dbo.T_ARABICDARIJAENTRY", "IsDeleted");
            DropColumn("dbo.T_ARABICDARIJAENTRY_TEXTENTITY", "IsDeleted");
            DropColumn("dbo.T_ARABICDARIJAENTRY_LATINWORD", "IsDeleted");
        }
    }
}
