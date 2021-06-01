using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CxpProductIdonparent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CxpProductId",
                table: "GameParents",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CxpProductId",
                table: "GameParents");
        }
    }
}
