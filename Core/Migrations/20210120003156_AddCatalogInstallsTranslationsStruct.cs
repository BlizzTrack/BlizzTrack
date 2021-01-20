using System.Collections.Generic;
using Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class AddCatalogInstallsTranslationsStruct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<CatalogInstall>>(
                name: "Installs",
                table: "Catalogs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Translations",
                table: "Catalogs",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Installs",
                table: "Catalogs");

            migrationBuilder.DropColumn(
                name: "Translations",
                table: "Catalogs");
        }
    }
}
