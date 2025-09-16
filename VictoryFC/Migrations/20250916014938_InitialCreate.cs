using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoryFC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HomeTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: true),
                    AwayTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AwayScore = table.Column<int>(type: "int", nullable: true),
                    Competition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Round = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GameNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scorers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Competition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scorers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Competition_Date",
                table: "Matches",
                columns: new[] { "Competition", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Scorers_Competition_Goals",
                table: "Scorers",
                columns: new[] { "Competition", "Goals" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Scorers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
