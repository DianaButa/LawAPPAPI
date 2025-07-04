﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class dailyeventsnames4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LawyerName",
                table: "DailyEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LawyerName",
                table: "DailyEvents");
        }
    }
}
