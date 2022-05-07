using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brighid.Commands.Database
{
    public partial class AddCommandScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scopes",
                table: "Commands",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scopes",
                table: "Commands");
        }
    }
}
