using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class eventsclientid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ScheduledEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "ScheduledEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "ScheduledEvents");
        }
    }
}
