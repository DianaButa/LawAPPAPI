using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    /// <inheritdoc />
    public partial class not : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LawyerName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LawyerName",
                table: "Notifications");
        }
    }
}
