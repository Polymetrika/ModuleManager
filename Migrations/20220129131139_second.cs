using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleManager.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Processes",
                newName: "ReleaseStatus");

            migrationBuilder.AddColumn<int>(
                name: "ReleaseStatus",
                table: "Templates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseStatus",
                table: "Templates");

            migrationBuilder.RenameColumn(
                name: "ReleaseStatus",
                table: "Processes",
                newName: "Status");
        }
    }
}
