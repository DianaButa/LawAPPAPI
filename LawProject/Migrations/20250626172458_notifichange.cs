using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    /// <inheritdoc />
    public partial class notifichange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HearingDate",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_LawyerId",
                table: "Tasks",
                column: "LawyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Lawyers_LawyerId",
                table: "Tasks",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Lawyers_LawyerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_LawyerId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HearingDate",
                table: "Notifications");
        }
    }
}
