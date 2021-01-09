using Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class GameConfigOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_configs",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Config = table.Column<ConfigItems>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_configs", x => x.Code);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_configs");
        }
    }
}