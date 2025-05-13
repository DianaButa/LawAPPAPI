using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class eventsreported : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReported",
                table: "ScheduledEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReported",
                table: "EventsC",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReported",
                table: "EventsA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EventAId",
                table: "DailyEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventCId",
                table: "DailyEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledEventId",
                table: "DailyEvents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvents_LawyerId",
                table: "DailyEvents",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyEvents_Lawyers_LawyerId",
                table: "DailyEvents",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyEvents_Lawyers_LawyerId",
                table: "DailyEvents");

            migrationBuilder.DropIndex(
                name: "IX_DailyEvents_LawyerId",
                table: "DailyEvents");

            migrationBuilder.DropColumn(
                name: "IsReported",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "IsReported",
                table: "EventsC");

            migrationBuilder.DropColumn(
                name: "IsReported",
                table: "EventsA");

            migrationBuilder.DropColumn(
                name: "EventAId",
                table: "DailyEvents");

            migrationBuilder.DropColumn(
                name: "EventCId",
                table: "DailyEvents");

            migrationBuilder.DropColumn(
                name: "ScheduledEventId",
                table: "DailyEvents");
        }
    }
}
