using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class dailyeventsnames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "DailyEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "DailyEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "DailyEvents");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "DailyEvents");
        }
    }
}
