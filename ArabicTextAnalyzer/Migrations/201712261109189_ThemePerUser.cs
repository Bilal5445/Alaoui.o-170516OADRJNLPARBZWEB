namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ThemePerUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_XTRCTTHEME", "UserID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_XTRCTTHEME", "UserID");
        }
    }
}
