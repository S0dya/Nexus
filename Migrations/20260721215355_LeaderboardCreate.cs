using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Migrations
{
    /// <inheritdoc />
    public partial class LeaderboardCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaderboardEntryEntities",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BestScore = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntryEntities", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntryEntities_BestScore_LastUpdated",
                table: "LeaderboardEntryEntities",
                columns: new[] { "BestScore", "LastUpdated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardEntryEntities");
        }
    }
}
