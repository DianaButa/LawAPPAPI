using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class contract : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientAdress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CNP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CUI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Onorariu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scadenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Obiect = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
