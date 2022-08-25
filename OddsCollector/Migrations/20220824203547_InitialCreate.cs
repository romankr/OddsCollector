using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OddsCollector.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SportEvents",
                columns: table => new
                {
                    SportEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommenceTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwayTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportEvents", x => x.SportEventId);
                });

            migrationBuilder.CreateTable(
                name: "Odds",
                columns: table => new
                {
                    Bookmaker = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SportEventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeOdd = table.Column<double>(type: "float", nullable: false),
                    DrawOdd = table.Column<double>(type: "float", nullable: false),
                    AwayOdd = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Odds", x => new { x.SportEventId, x.LastUpdate, x.Bookmaker });
                    table.ForeignKey(
                        name: "FK_Odds_SportEvents_SportEventId",
                        column: x => x.SportEventId,
                        principalTable: "SportEvents",
                        principalColumn: "SportEventId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Odds");

            migrationBuilder.DropTable(
                name: "SportEvents");
        }
    }
}
