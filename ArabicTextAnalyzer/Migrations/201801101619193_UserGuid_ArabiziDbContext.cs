namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserGuid_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegisterUser", "UserGuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegisterUser", "UserGuid");
        }
    }
}
