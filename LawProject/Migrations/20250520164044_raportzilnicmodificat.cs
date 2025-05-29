using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class raportzilnicmodificat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rapoarte_ClientPFs_ClientPFId",
                table: "Rapoarte");

            migrationBuilder.DropForeignKey(
                name: "FK_Rapoarte_ClientPJs_ClientPJId",
                table: "Rapoarte");

            migrationBuilder.DropIndex(
                name: "IX_Rapoarte_ClientPFId",
                table: "Rapoarte");

            migrationBuilder.DropIndex(
                name: "IX_Rapoarte_ClientPJId",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "ClientPFId",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "ClientPJId",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "Rapoarte");

            migrationBuilder.AddColumn<double>(
                name: "OreAlteActivitati",
                table: "Rapoarte",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OreAudieri",
                table: "Rapoarte",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OreConsultante",
                table: "Rapoarte",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OreInstanta",
                table: "Rapoarte",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "RaportStudiuDosar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    FileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OreStudiu = table.Column<double>(type: "float", nullable: false),
                    RaportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaportStudiuDosar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaportStudiuDosar_Rapoarte_RaportId",
                        column: x => x.RaportId,
                        principalTable: "Rapoarte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaportStudiuDosar_RaportId",
                table: "RaportStudiuDosar",
                column: "RaportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaportStudiuDosar");

            migrationBuilder.DropColumn(
                name: "OreAlteActivitati",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "OreAudieri",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "OreConsultante",
                table: "Rapoarte");

            migrationBuilder.DropColumn(
                name: "OreInstanta",
                table: "Rapoarte");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Rapoarte",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Rapoarte",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientPFId",
                table: "Rapoarte",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientPJId",
                table: "Rapoarte",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "Rapoarte",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_ClientPFId",
                table: "Rapoarte",
                column: "ClientPFId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_ClientPJId",
                table: "Rapoarte",
                column: "ClientPJId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rapoarte_ClientPFs_ClientPFId",
                table: "Rapoarte",
                column: "ClientPFId",
                principalTable: "ClientPFs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rapoarte_ClientPJs_ClientPJId",
                table: "Rapoarte",
                column: "ClientPJId",
                principalTable: "ClientPJs",
                principalColumn: "Id");
        }
    }
}
