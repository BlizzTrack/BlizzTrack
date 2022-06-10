using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class givengameseqn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GivenSeqn",
                table: "versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GivenSeqn",
                table: "summary",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GivenSeqn",
                table: "cdns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GivenSeqn",
                table: "bgdl",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GivenSeqn",
                table: "versions");

            migrationBuilder.DropColumn(
                name: "GivenSeqn",
                table: "summary");

            migrationBuilder.DropColumn(
                name: "GivenSeqn",
                table: "cdns");

            migrationBuilder.DropColumn(
                name: "GivenSeqn",
                table: "bgdl");
        }
    }
}
