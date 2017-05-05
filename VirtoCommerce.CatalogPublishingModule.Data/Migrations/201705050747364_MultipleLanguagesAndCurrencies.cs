namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultipleLanguagesAndCurrencies : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReadinessChannelCurrencyEntity",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CurrencyCode = c.String(nullable: false, maxLength: 64),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReadinessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.ReadinessChannelLanguageEntity",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        LanguageCode = c.String(nullable: false),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReadinessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
            AlterColumn("dbo.ReadinessChannel", "EvaluatorType", c => c.String(nullable: false));
            DropColumn("dbo.ReadinessChannel", "Language");
            DropColumn("dbo.ReadinessChannel", "PricelistId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReadinessChannel", "PricelistId", c => c.String(maxLength: 128));
            AddColumn("dbo.ReadinessChannel", "Language", c => c.String());
            DropForeignKey("dbo.ReadinessChannelLanguageEntity", "ChannelId", "dbo.ReadinessChannel");
            DropForeignKey("dbo.ReadinessChannelCurrencyEntity", "ChannelId", "dbo.ReadinessChannel");
            DropIndex("dbo.ReadinessChannelLanguageEntity", new[] { "ChannelId" });
            DropIndex("dbo.ReadinessChannelCurrencyEntity", new[] { "ChannelId" });
            AlterColumn("dbo.ReadinessChannel", "EvaluatorType", c => c.String());
            DropTable("dbo.ReadinessChannelLanguageEntity");
            DropTable("dbo.ReadinessChannelCurrencyEntity");
        }
    }
}
