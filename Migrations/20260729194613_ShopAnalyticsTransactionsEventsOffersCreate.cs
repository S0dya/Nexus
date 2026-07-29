using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Migrations
{
    /// <inheritdoc />
    public partial class ShopAnalyticsTransactionsEventsOffersCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coins = table.Column<int>(type: "integer", nullable: false),
                    Gems = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => new { x.UserId, x.ItemId });
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: true),
                    CurrencyAmount = table.Column<int>(type: "integer", nullable: true),
                    ItemId = table.Column<string>(type: "text", nullable: true),
                    ItemAmount = table.Column<int>(type: "integer", nullable: true),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAnalytics",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purchases = table.Column<int>(type: "integer", nullable: false),
                    CoinsSpent = table.Column<int>(type: "integer", nullable: false),
                    ItemsBought = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAnalytics", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ShopOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<string>(type: "text", nullable: false),
                    PriceCurrency = table.Column<int>(type: "integer", nullable: false),
                    PriceAmount = table.Column<int>(type: "integer", nullable: false),
                    RewardItemId = table.Column<string>(type: "text", nullable: false),
                    RewardAmount = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOffers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_ProcessedAt_CreatedAt",
                table: "GameEvents",
                columns: new[] { "ProcessedAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_UserId_CreatedAt",
                table: "InventoryTransactions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnalytics_UserId",
                table: "PlayerAnalytics",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopOffers_OfferId",
                table: "ShopOffers",
                column: "OfferId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameEvents");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "PlayerAnalytics");

            migrationBuilder.DropTable(
                name: "ShopOffers");
        }
    }
}
