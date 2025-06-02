using Microsoft.EntityFrameworkCore.Migrations;



#nullable disable

namespace LawProject.Migrations
{
    public partial class stornare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStorned",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StornedInvoiceId",
                table: "Invoices",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsStorned",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StornedInvoiceId",
                table: "Invoices");
        }
    }
}
