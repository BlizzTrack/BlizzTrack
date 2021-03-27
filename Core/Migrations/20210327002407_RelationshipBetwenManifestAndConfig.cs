using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class RelationshipBetwenManifestAndConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigId",
                table: "versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigId",
                table: "summary",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigId",
                table: "cdns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigId",
                table: "bgdl",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_versions_ConfigId",
                table: "versions",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_cdns_ConfigId",
                table: "cdns",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_bgdl_ConfigId",
                table: "bgdl",
                column: "ConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_bgdl_game_configs_ConfigId",
                table: "bgdl",
                column: "ConfigId",
                principalTable: "game_configs",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_cdns_game_configs_ConfigId",
                table: "cdns",
                column: "ConfigId",
                principalTable: "game_configs",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_versions_game_configs_ConfigId",
                table: "versions",
                column: "ConfigId",
                principalTable: "game_configs",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bgdl_game_configs_ConfigId",
                table: "bgdl");

            migrationBuilder.DropForeignKey(
                name: "FK_cdns_game_configs_ConfigId",
                table: "cdns");

            migrationBuilder.DropForeignKey(
                name: "FK_versions_game_configs_ConfigId",
                table: "versions");

            migrationBuilder.DropIndex(
                name: "IX_versions_ConfigId",
                table: "versions");

            migrationBuilder.DropIndex(
                name: "IX_cdns_ConfigId",
                table: "cdns");

            migrationBuilder.DropIndex(
                name: "IX_bgdl_ConfigId",
                table: "bgdl");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "versions");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "summary");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "cdns");

            migrationBuilder.DropColumn(
                name: "ConfigId",
                table: "bgdl");
        }
    }
}
