using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class PatchNotesBaseExtended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PatchNotes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PatchNotes",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "PatchNotes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PatchNotes");
        }
    }
}