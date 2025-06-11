using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    /// <inheritdoc />
    public partial class emaillawyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Lawyers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "LawyerId",
                table: "Files",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Lawyers");

            migrationBuilder.AlterColumn<int>(
                name: "LawyerId",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
