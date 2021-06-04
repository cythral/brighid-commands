using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Commands.Database
{
    public partial class RequiredRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredRole",
                table: "Commands",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredRole",
                table: "Commands");
        }
    }
}
