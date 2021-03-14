using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class LanguageSupportInPatchNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PatchNotes",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "PatchNotes");
        }
    }
}
