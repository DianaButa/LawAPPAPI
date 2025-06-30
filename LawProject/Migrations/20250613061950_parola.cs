using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    /// <inheritdoc />
    public partial class parola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parola",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parola",
                table: "Files");
        }
    }
}
