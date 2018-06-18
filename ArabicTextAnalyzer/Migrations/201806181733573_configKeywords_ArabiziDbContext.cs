namespace ArabicTextAnalyzer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class configKeywords_ArabiziDbContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.T_XTRCTTHEME_CONFIG_KEYWORD",
                c => new
                    {
                        ID_XTRCTTHEME_CONFIG_KEYWORD = c.Guid(nullable: false),
                        ID_XTRCTTHEME = c.Guid(nullable: false),
                        Keyword = c.String(),
                        IsDeleted = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID_XTRCTTHEME_CONFIG_KEYWORD);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.T_XTRCTTHEME_CONFIG_KEYWORD");
        }
    }
}
