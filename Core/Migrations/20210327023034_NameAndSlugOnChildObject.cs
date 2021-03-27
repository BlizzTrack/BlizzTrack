using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class NameAndSlugOnChildObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GameChildren",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "GameChildren",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "GameChildren");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "GameChildren");
        }
    }
}
