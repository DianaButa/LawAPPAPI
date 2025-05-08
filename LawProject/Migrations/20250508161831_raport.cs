using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class raport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rapoarte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerId = table.Column<int>(type: "int", nullable: false),
                    LawyerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ClientPFId = table.Column<int>(type: "int", nullable: true),
                    ClientPJId = table.Column<int>(type: "int", nullable: true),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    FileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRaport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkTaskId = table.Column<int>(type: "int", nullable: true),
                    OreDeplasare = table.Column<double>(type: "float", nullable: false),
                    OreStudiu = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rapoarte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rapoarte_ClientPFs_ClientPFId",
                        column: x => x.ClientPFId,
                        principalTable: "ClientPFs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rapoarte_ClientPJs_ClientPJId",
                        column: x => x.ClientPJId,
                        principalTable: "ClientPJs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rapoarte_Lawyers_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "Lawyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rapoarte_Tasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RaportTaskuri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaportId = table.Column<int>(type: "int", nullable: false),
                    WorkTaskId = table.Column<int>(type: "int", nullable: false),
                    OreLucrate = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaportTaskuri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaportTaskuri_Rapoarte_RaportId",
                        column: x => x.RaportId,
                        principalTable: "Rapoarte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaportTaskuri_Tasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_ClientPFId",
                table: "Rapoarte",
                column: "ClientPFId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_ClientPJId",
                table: "Rapoarte",
                column: "ClientPJId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_LawyerId",
                table: "Rapoarte",
                column: "LawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rapoarte_WorkTaskId",
                table: "Rapoarte",
                column: "WorkTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_RaportTaskuri_RaportId",
                table: "RaportTaskuri",
                column: "RaportId");

            migrationBuilder.CreateIndex(
                name: "IX_RaportTaskuri_WorkTaskId",
                table: "RaportTaskuri",
                column: "WorkTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaportTaskuri");

            migrationBuilder.DropTable(
                name: "Rapoarte");
        }
    }
}
