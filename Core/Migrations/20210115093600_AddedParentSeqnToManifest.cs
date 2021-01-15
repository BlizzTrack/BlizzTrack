using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddedParentSeqnToManifest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Parent",
                table: "versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Parent",
                table: "cdns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Parent",
                table: "bgdl",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parent",
                table: "versions");

            migrationBuilder.DropColumn(
                name: "Parent",
                table: "cdns");

            migrationBuilder.DropColumn(
                name: "Parent",
                table: "bgdl");
        }
    }
}
