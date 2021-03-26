using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    public partial class AddedGameCompanyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "GameParents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompanies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameParents_OwnerId",
                table: "GameParents",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameParents_GameCompanies_OwnerId",
                table: "GameParents",
                column: "OwnerId",
                principalTable: "GameCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameParents_GameCompanies_OwnerId",
                table: "GameParents");

            migrationBuilder.DropTable(
                name: "GameCompanies");

            migrationBuilder.DropIndex(
                name: "IX_GameParents_OwnerId",
                table: "GameParents");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "GameParents");
        }
    }
}
