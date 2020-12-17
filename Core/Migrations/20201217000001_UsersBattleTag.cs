using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class UsersBattleTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BattleTag",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BattleTag",
                table: "AspNetUsers");
        }
    }
}
