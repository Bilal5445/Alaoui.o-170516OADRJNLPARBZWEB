namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ThemeKeyWordWithCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_XTRCTTHEME_KEYWORD", "Keyword_Count", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_XTRCTTHEME_KEYWORD", "Keyword_Count");
        }
    }
}
