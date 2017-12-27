namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ThemeKeyWordWithType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_XTRCTTHEME_KEYWORD", "Keyword_Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_XTRCTTHEME_KEYWORD", "Keyword_Type");
        }
    }
}
