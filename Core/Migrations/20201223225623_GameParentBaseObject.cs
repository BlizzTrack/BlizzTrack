using System.Collections.Generic;
using Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class GameParentBaseObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameParents",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ChildrenOverride = table.Column<List<string>>(type: "text[]", nullable: true),
                    Logos = table.Column<List<Icons>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameParents", x => x.Code);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameParents");
        }
    }
}
