namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastLoginTime_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegisterUser", "LastLoginTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegisterUser", "LastLoginTime");
        }
    }
}
