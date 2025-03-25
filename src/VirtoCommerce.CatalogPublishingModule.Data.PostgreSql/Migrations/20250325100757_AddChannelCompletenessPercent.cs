using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogPublishingModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelCompletenessPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CompletenessPercent",
                table: "CompletenessEntry",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletenessPercent",
                table: "CompletenessDetail",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "CompletenessPercent",
                table: "CompletenessChannel",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletenessPercent",
                table: "CompletenessChannel");

            migrationBuilder.AlterColumn<int>(
                name: "CompletenessPercent",
                table: "CompletenessEntry",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "CompletenessPercent",
                table: "CompletenessDetail",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
