namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArabiziTheme : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_ARABIZIENTRY", "ID_XTRCTTHEME", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_ARABIZIENTRY", "ID_XTRCTTHEME");
        }
    }
}
