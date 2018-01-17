namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArabiziIsFR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_ARABIZIENTRY", "IsFR", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_ARABIZIENTRY", "IsFR");
        }
    }
}
