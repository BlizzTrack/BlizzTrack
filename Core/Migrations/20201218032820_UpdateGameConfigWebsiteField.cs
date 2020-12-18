using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class UpdateGameConfigWebsiteField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "game_configs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Website",
                table: "game_configs");
        }
    }
}
