using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CatalogPublishingModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletenessChannel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CatalogId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CatalogName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EvaluatorType = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessChannelCurrency",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessChannelCurrency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletenessChannelCurrency_CompletenessChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "CompletenessChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessChannelLanguage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessChannelLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletenessChannelLanguage_CompletenessChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "CompletenessChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessEntry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProductId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CompletenessPercent = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletenessEntry_CompletenessChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "CompletenessChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CompletenessPercent = table.Column<int>(type: "integer", nullable: false),
                    CompletenessEntryId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletenessDetail_CompletenessEntry_CompletenessEntryId",
                        column: x => x.CompletenessEntryId,
                        principalTable: "CompletenessEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessChannelCurrency_ChannelId",
                table: "CompletenessChannelCurrency",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessChannelLanguage_ChannelId",
                table: "CompletenessChannelLanguage",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessDetail_CompletenessEntryId",
                table: "CompletenessDetail",
                column: "CompletenessEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessEntry_ChannelId",
                table: "CompletenessEntry",
                column: "ChannelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletenessChannelCurrency");

            migrationBuilder.DropTable(
                name: "CompletenessChannelLanguage");

            migrationBuilder.DropTable(
                name: "CompletenessDetail");

            migrationBuilder.DropTable(
                name: "CompletenessEntry");

            migrationBuilder.DropTable(
                name: "CompletenessChannel");
        }
    }
}
