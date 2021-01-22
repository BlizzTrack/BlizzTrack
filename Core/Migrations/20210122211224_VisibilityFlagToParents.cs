using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class VisibilityFlagToParents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "GameParents",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visible",
                table: "GameParents");
        }
    }
}
