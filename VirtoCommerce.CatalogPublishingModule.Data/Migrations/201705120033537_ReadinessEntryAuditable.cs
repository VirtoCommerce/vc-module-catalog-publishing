namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReadinessEntryAuditable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReadinessEntry", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ReadinessEntry", "ModifiedDate", c => c.DateTime());
            AddColumn("dbo.ReadinessEntry", "CreatedBy", c => c.String(maxLength: 64));
            AddColumn("dbo.ReadinessEntry", "ModifiedBy", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReadinessEntry", "ModifiedBy");
            DropColumn("dbo.ReadinessEntry", "CreatedBy");
            DropColumn("dbo.ReadinessEntry", "ModifiedDate");
            DropColumn("dbo.ReadinessEntry", "CreatedDate");
        }
    }
}
