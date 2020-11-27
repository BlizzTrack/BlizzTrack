using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class removedindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_versions_Code_Seqn",
                table: "versions");

            migrationBuilder.DropIndex(
                name: "IX_summary_Code_Seqn",
                table: "summary");

            migrationBuilder.DropIndex(
                name: "IX_cdns_Code_Seqn",
                table: "cdns");

            migrationBuilder.DropIndex(
                name: "IX_bgdl_Code_Seqn",
                table: "bgdl");
        }   

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_versions_Code_Seqn",
                table: "versions",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_summary_Code_Seqn",
                table: "summary",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cdns_Code_Seqn",
                table: "cdns",
                columns: new[] { "Code", "Seqn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bgdl_Code_Seqn",
                table: "bgdl",
                columns: new[] { "Code", "Seqn" },
                unique: true);
        }
    }
}
