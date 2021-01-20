using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AdCatalogNameStruct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Catalogs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Catalogs");
        }
    }
}
