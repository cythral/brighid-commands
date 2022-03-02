using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Commands.Database
{
    public partial class EmbeddedLocationGrouping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbeddedLocation",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("update Commands set EmbeddedLocation=json_object('AssemblyName', AssemblyName, 'Checksum', Checksum, 'DownloadURL', DownloadURL, 'TypeName', TypeName)");

            migrationBuilder.DropColumn(
                name: "AssemblyName",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "Checksum",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "DownloadURL",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "Commands");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbeddedLocation",
                table: "Commands");

            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyName",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Checksum",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DownloadURL",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
