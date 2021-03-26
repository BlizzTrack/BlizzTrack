using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddedGameChildren3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameChildren_GameParents_Code",
                table: "GameChildren");

            migrationBuilder.DropIndex(
                name: "IX_GameChildren_Code",
                table: "GameChildren");

            migrationBuilder.AddColumn<string>(
                name: "ParentCode",
                table: "GameChildren",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "game_configs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameChildren_ParentCode",
                table: "GameChildren",
                column: "ParentCode");

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

            migrationBuilder.AddForeignKey(
                name: "FK_GameChildren_GameParents_ParentCode",
                table: "GameChildren",
                column: "ParentCode",
                principalTable: "GameParents",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_configs_GameChildren_OwnerId",
                table: "game_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChildren_GameParents_ParentCode",
                table: "GameChildren");

            migrationBuilder.DropIndex(
                name: "IX_GameChildren_ParentCode",
                table: "GameChildren");

            migrationBuilder.DropIndex(
                name: "IX_game_configs_OwnerId",
                table: "game_configs");

            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "GameChildren");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "game_configs");

            migrationBuilder.CreateIndex(
                name: "IX_GameChildren_Code",
                table: "GameChildren",
                column: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChildren_GameParents_Code",
                table: "GameChildren",
                column: "Code",
                principalTable: "GameParents",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
