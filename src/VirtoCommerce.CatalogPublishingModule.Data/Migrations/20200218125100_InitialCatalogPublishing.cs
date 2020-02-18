﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CatalogPublishingModule.Data.Migrations
{
    public partial class InitialCatalogPublishing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletenessChannel",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    CatalogId = table.Column<string>(maxLength: 128, nullable: false),
                    CatalogName = table.Column<string>(maxLength: 128, nullable: false),
                    EvaluatorType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessChannelCurrency",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CurrencyCode = table.Column<string>(maxLength: 64, nullable: false),
                    ChannelId = table.Column<string>(nullable: false),
                    CompletenessChannelEntityId = table.Column<string>(nullable: true)
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
                    table.ForeignKey(
                        name: "FK_CompletenessChannelCurrency_CompletenessChannel_CompletenessChannelEntityId",
                        column: x => x.CompletenessChannelEntityId,
                        principalTable: "CompletenessChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessChannelLanguage",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(nullable: false),
                    ChannelId = table.Column<string>(nullable: false),
                    CompletenessChannelEntityId = table.Column<string>(nullable: true)
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
                    table.ForeignKey(
                        name: "FK_CompletenessChannelLanguage_CompletenessChannel_CompletenessChannelEntityId",
                        column: x => x.CompletenessChannelEntityId,
                        principalTable: "CompletenessChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompletenessEntry",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ProductId = table.Column<string>(maxLength: 128, nullable: false),
                    CompletenessPercent = table.Column<int>(nullable: false),
                    ChannelId = table.Column<string>(nullable: false)
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
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    CompletenessPercent = table.Column<int>(nullable: false),
                    CompletenessEntryId = table.Column<string>(nullable: false),
                    CompletenessEntryEntityId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletenessDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletenessDetail_CompletenessEntry_CompletenessEntryEntityId",
                        column: x => x.CompletenessEntryEntityId,
                        principalTable: "CompletenessEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_CompletenessChannelCurrency_CompletenessChannelEntityId",
                table: "CompletenessChannelCurrency",
                column: "CompletenessChannelEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessChannelLanguage_ChannelId",
                table: "CompletenessChannelLanguage",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessChannelLanguage_CompletenessChannelEntityId",
                table: "CompletenessChannelLanguage",
                column: "CompletenessChannelEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletenessDetail_CompletenessEntryEntityId",
                table: "CompletenessDetail",
                column: "CompletenessEntryEntityId");

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
