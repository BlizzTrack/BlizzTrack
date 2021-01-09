using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class GameParentsPatchNotesCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatchNoteCode",
                table: "GameParents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatchNoteCode",
                table: "GameParents");
        }
    }
}