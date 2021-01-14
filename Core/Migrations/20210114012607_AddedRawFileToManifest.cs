using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddedRawFileToManifest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "summary",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "cdns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Raw",
                table: "bgdl",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Raw",
                table: "versions");

            migrationBuilder.DropColumn(
                name: "Raw",
                table: "summary");

            migrationBuilder.DropColumn(
                name: "Raw",
                table: "cdns");

            migrationBuilder.DropColumn(
                name: "Raw",
                table: "bgdl");
        }
    }
}
