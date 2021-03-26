using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddedGameChildren : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameChildren",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    ParentCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChildren", x => x.Code);
                    table.ForeignKey(
                        name: "FK_GameChildren_GameParents_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "GameParents",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameChildren_ParentCode",
                table: "GameChildren",
                column: "ParentCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameChildren");
        }
    }
}
