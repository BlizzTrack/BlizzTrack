using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddedGameChildren4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_configs_GameChildren_OwnerId",
                table: "game_configs");

            migrationBuilder.DropIndex(
                name: "IX_game_configs_OwnerId",
                table: "game_configs");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "game_configs");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GameChildren_Code",
                table: "GameChildren",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_game_configs_GameChildren_Code",
                table: "game_configs",
                column: "Code",
                principalTable: "GameChildren",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_configs_GameChildren_Code",
                table: "game_configs");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_GameChildren_Code",
                table: "GameChildren");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "game_configs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_configs_OwnerId",
                table: "game_configs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_game_configs_GameChildren_OwnerId",
                table: "game_configs",
                column: "OwnerId",
                principalTable: "GameChildren",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
