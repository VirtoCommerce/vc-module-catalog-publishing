using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    public partial class UpdateCatalogPublishingV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
                IF (EXISTS (SELECT * FROM __MigrationHistory WHERE ContextKey = 'VirtoCommerce.CatalogPublishingModule.Data.Data.Migrations.Configuration'))
                    BEGIN
                        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId],[ProductVersion]) VALUES ('20200218125100_InitialCatalogPublishing', '2.2.3-servicing-35854')
                    END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not needed
        }
    }
}
