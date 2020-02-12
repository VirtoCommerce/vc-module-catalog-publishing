namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompletenessDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        CompletenessPercent = c.Int(nullable: false),
                        CompletenessEntryId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompletenessEntry", t => t.CompletenessEntryId, cascadeDelete: true)
                .Index(t => t.CompletenessEntryId);
            
            CreateTable(
                "dbo.CompletenessEntry",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        CompletenessPercent = c.Int(nullable: false),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompletenessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.CompletenessChannel",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        CatalogId = c.String(nullable: false, maxLength: 128),
                        CatalogName = c.String(nullable: false, maxLength: 128),
                        EvaluatorType = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CompletenessChannelCurrency",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CurrencyCode = c.String(nullable: false, maxLength: 64),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompletenessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.CompletenessChannelLanguage",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        LanguageCode = c.String(nullable: false),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompletenessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompletenessDetail", "CompletenessEntryId", "dbo.CompletenessEntry");
            DropForeignKey("dbo.CompletenessEntry", "ChannelId", "dbo.CompletenessChannel");
            DropForeignKey("dbo.CompletenessChannelLanguage", "ChannelId", "dbo.CompletenessChannel");
            DropForeignKey("dbo.CompletenessChannelCurrency", "ChannelId", "dbo.CompletenessChannel");
            DropIndex("dbo.CompletenessChannelLanguage", new[] { "ChannelId" });
            DropIndex("dbo.CompletenessChannelCurrency", new[] { "ChannelId" });
            DropIndex("dbo.CompletenessEntry", new[] { "ChannelId" });
            DropIndex("dbo.CompletenessDetail", new[] { "CompletenessEntryId" });
            DropTable("dbo.CompletenessChannelLanguage");
            DropTable("dbo.CompletenessChannelCurrency");
            DropTable("dbo.CompletenessChannel");
            DropTable("dbo.CompletenessEntry");
            DropTable("dbo.CompletenessDetail");
        }
    }
}
