using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brighid.Commands.Database
{
    public partial class VersionDefaultValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update Commands set Version='1'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
