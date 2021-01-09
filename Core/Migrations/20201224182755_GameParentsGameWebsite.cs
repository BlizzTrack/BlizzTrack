using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class GameParentsGameWebsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "GameParents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Website",
                table: "GameParents");
        }
    }
}