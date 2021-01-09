using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

namespace Core.Migrations
{
    public partial class GameParentsPatchNotesOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "PatchNoteAreas",
                table: "GameParents",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatchNoteTool",
                table: "GameParents",
                type: "text",
                nullable: true,
                defaultValue: "legacy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatchNoteAreas",
                table: "GameParents");

            migrationBuilder.DropColumn(
                name: "PatchNoteTool",
                table: "GameParents");
        }
    }
}