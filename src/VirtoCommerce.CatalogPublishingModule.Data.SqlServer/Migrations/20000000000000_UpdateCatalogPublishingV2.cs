using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPublishingModule.Data.SqlServer.Migrations
{
    public partial class UpdateCatalogPublishingV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                IF (EXISTS (SELECT 1 FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogPublishingModule.Data.Migrations.Configuration'))
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('20200220125258_InitialCatalogPublishing', '2.2.3-servicing-35854')
                    END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not needed
        }
    }
}
