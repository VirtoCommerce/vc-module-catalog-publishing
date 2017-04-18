namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReadinessDetail",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        ReadinessPercent = c.Int(nullable: false),
                        ReadinessEntryId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReadinessEntry", t => t.ReadinessEntryId, cascadeDelete: true)
                .Index(t => t.ReadinessEntryId);
            
            CreateTable(
                "dbo.ReadinessEntry",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(nullable: false, maxLength: 128),
                        ReadinessPercent = c.Int(nullable: false),
                        ChannelId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReadinessChannel", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.ReadinessChannel",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        CatalogId = c.String(nullable: false, maxLength: 128),
                        Language = c.String(),
                        PricelistId = c.String(maxLength: 128),
                        EvaluatorType = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 64),
                        ModifiedBy = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReadinessDetail", "ReadinessEntryId", "dbo.ReadinessEntry");
            DropForeignKey("dbo.ReadinessEntry", "ChannelId", "dbo.ReadinessChannel");
            DropIndex("dbo.ReadinessEntry", new[] { "ChannelId" });
            DropIndex("dbo.ReadinessDetail", new[] { "ReadinessEntryId" });
            DropTable("dbo.ReadinessChannel");
            DropTable("dbo.ReadinessEntry");
            DropTable("dbo.ReadinessDetail");
        }
    }
}
