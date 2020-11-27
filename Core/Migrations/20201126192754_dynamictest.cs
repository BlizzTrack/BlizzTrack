using System;
using BNetLib.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    public partial class dynamictest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manifests");

            migrationBuilder.CreateTable(
                name: "bgdl",
                columns: table => new
                {
                    Seqn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<BGDL[]>(type: "jsonb", nullable: true),
                    Indexed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bgdl", x => x.Seqn);
                });

            migrationBuilder.CreateTable(
                name: "cdns",
                columns: table => new
                {
                    Seqn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<CDN[]>(type: "jsonb", nullable: true),
                    Indexed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cdns", x => x.Seqn);
                });

            migrationBuilder.CreateTable(
                name: "summary",
                columns: table => new
                {
                    Seqn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<Summary[]>(type: "jsonb", nullable: true),
                    Indexed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_summary", x => x.Seqn);
                });

            migrationBuilder.CreateTable(
                name: "versions",
                columns: table => new
                {
                    Seqn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<Versions[]>(type: "jsonb", nullable: true),
                    Indexed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_versions", x => x.Seqn);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bgdl_Code_Seqn",
                table: "bgdl",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdns_Code_Seqn",
                table: "cdns",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_summary_Code_Seqn",
                table: "summary",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_versions_Code_Seqn",
                table: "versions",
                columns: new[] { "Code", "Seqn" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bgdl");

            migrationBuilder.DropTable(
                name: "cdns");

            migrationBuilder.DropTable(
                name: "summary");

            migrationBuilder.DropTable(
                name: "versions");

            migrationBuilder.CreateTable(
                name: "manifests",
                columns: table => new
                {
                    Seqn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    File = table.Column<string>(type: "text", nullable: true),
                    Indexed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifests", x => x.Seqn);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manifests_Code_Seqn_File",
                table: "manifests",
                columns: new[] { "Code", "Seqn", "File" },
                unique: true);
        }
    }
}
