using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class events1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledEvents_Lawyers_LawyerId",
                table: "ScheduledEvents");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledEvents_LawyerId",
                table: "ScheduledEvents");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "ScheduledEvents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LawyerId",
                table: "ScheduledEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEvents_LawyerId",
                table: "ScheduledEvents",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledEvents_Lawyers_LawyerId",
                table: "ScheduledEvents",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
