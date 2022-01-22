using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleManager.Migrations
{
    public partial class update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classification",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "Reviews",
                table: "Module");

            migrationBuilder.RenameColumn(
                name: "Template",
                table: "Module",
                newName: "TemplateId");

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "Templates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.ReviewId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "Templates");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "Module",
                newName: "Template");

            migrationBuilder.AddColumn<string>(
                name: "Classification",
                table: "Module",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reviews",
                table: "Module",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
