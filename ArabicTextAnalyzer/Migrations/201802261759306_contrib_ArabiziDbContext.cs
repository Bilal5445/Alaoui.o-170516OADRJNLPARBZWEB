namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class contrib_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.T_ARABICDARIJAENTRY", "ContribArabicDarijaText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.T_ARABICDARIJAENTRY", "ContribArabicDarijaText");
        }
    }
}
