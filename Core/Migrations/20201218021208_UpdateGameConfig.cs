using System.Collections.Generic;
using Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class UpdateGameConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Icons>>(
                name: "Logos",
                table: "game_configs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "game_configs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logos",
                table: "game_configs");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "game_configs");
        }
    }
}
