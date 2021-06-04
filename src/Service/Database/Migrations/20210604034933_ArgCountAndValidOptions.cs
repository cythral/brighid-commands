using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Commands.Database
{
    public partial class ArgCountAndValidOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "ArgCount",
                table: "Commands",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "ValidOptions",
                table: "Commands",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArgCount",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "ValidOptions",
                table: "Commands");
        }
    }
}
