using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    /// <inheritdoc />
    public partial class notif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifiedForSolution",
                table: "ScheduledEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Solutie",
                table: "ScheduledEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutieSumar",
                table: "ScheduledEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifiedForSolution",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "Solutie",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "SolutieSumar",
                table: "ScheduledEvents");
        }
    }
}
