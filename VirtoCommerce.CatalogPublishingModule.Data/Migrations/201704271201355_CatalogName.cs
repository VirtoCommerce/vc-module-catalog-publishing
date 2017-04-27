namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CatalogName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReadinessChannel", "CatalogName", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReadinessChannel", "CatalogName");
        }
    }
}
