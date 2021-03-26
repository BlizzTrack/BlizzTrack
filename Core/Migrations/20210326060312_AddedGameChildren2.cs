using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    public partial class AddedGameChildren2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameChildren_GameParents_ParentCode",
                table: "GameChildren");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameChildren",
                table: "GameChildren");

            migrationBuilder.DropIndex(
                name: "IX_GameChildren_ParentCode",
                table: "GameChildren");

            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "GameChildren");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "GameChildren",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameChildren",
                table: "GameChildren",
                column: "Id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameChildren_GameParents_Code",
                table: "GameChildren");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameChildren",
                table: "GameChildren");

            migrationBuilder.DropIndex(
                name: "IX_GameChildren_Code",
                table: "GameChildren");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GameChildren");

            migrationBuilder.AddColumn<string>(
                name: "ParentCode",
                table: "GameChildren",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameChildren",
                table: "GameChildren",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_GameChildren_ParentCode",
                table: "GameChildren",
                column: "ParentCode");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChildren_GameParents_ParentCode",
                table: "GameChildren",
                column: "ParentCode",
                principalTable: "GameParents",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
