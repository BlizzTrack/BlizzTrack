using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class SlugForGameName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "GameParents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "GameParents");
        }
    }
}
