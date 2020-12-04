using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    public partial class RemovedStaticKeyFromSeqn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_versions",
                table: "versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cdns",
                table: "cdns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bgdl",
                table: "bgdl");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "versions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "versions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "cdns",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "cdns",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "bgdl",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "bgdl",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_versions",
                table: "versions",
                columns: new[] { "Code", "Seqn" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_cdns",
                table: "cdns",
                columns: new[] { "Code", "Seqn" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_bgdl",
                table: "bgdl",
                columns: new[] { "Code", "Seqn" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_versions",
                table: "versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cdns",
                table: "cdns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bgdl",
                table: "bgdl");

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "versions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "versions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "cdns",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "cdns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Seqn",
                table: "bgdl",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "bgdl",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_versions",
                table: "versions",
                column: "Seqn");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cdns",
                table: "cdns",
                column: "Seqn");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bgdl",
                table: "bgdl",
                column: "Seqn");
        }
    }
}
