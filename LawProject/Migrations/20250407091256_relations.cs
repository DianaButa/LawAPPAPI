using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawProject.Migrations
{
    public partial class relations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_ClientPFs_ClientId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ClientId",
                table: "Files");

            migrationBuilder.AddColumn<int>(
                name: "ClientPFId",
                table: "Files",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientPJId",
                table: "Files",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LawyerId",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "ClientPJs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClientType",
                table: "ClientPFs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClientPFFiles",
                columns: table => new
                {
                    ClientPFId = table.Column<int>(type: "int", nullable: false),
                    MyFileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPFFiles", x => new { x.ClientPFId, x.MyFileId });
                    table.ForeignKey(
                        name: "FK_ClientPFFiles_ClientPFs_ClientPFId",
                        column: x => x.ClientPFId,
                        principalTable: "ClientPFs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientPFFiles_Files_MyFileId",
                        column: x => x.MyFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientPJFiles",
                columns: table => new
                {
                    ClientPJId = table.Column<int>(type: "int", nullable: false),
                    MyFileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPJFiles", x => new { x.ClientPJId, x.MyFileId });
                    table.ForeignKey(
                        name: "FK_ClientPJFiles_ClientPJs_ClientPJId",
                        column: x => x.ClientPJId,
                        principalTable: "ClientPJs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientPJFiles_Files_MyFileId",
                        column: x => x.MyFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LawyerFiles",
                columns: table => new
                {
                    LawyerId = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerFiles", x => new { x.LawyerId, x.FileId });
                    table.ForeignKey(
                        name: "FK_LawyerFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LawyerFiles_Lawyers_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "Lawyers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_ClientPFId",
                table: "Files",
                column: "ClientPFId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ClientPJId",
                table: "Files",
                column: "ClientPJId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_LawyerId",
                table: "Files",
                column: "LawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPFFiles_MyFileId",
                table: "ClientPFFiles",
                column: "MyFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPJFiles_MyFileId",
                table: "ClientPJFiles",
                column: "MyFileId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerFiles_FileId",
                table: "LawyerFiles",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_ClientPFs_ClientPFId",
                table: "Files",
                column: "ClientPFId",
                principalTable: "ClientPFs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_ClientPJs_ClientPJId",
                table: "Files",
                column: "ClientPJId",
                principalTable: "ClientPJs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files",
                column: "LawyerId",
                principalTable: "Lawyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_ClientPFs_ClientPFId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_ClientPJs_ClientPJId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Lawyers_LawyerId",
                table: "Files");

            migrationBuilder.DropTable(
                name: "ClientPFFiles");

            migrationBuilder.DropTable(
                name: "ClientPJFiles");

            migrationBuilder.DropTable(
                name: "LawyerFiles");

            migrationBuilder.DropIndex(
                name: "IX_Files_ClientPFId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ClientPJId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_LawyerId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ClientPFId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ClientPJId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "LawyerId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "ClientPJs");

            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "ClientPFs");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ClientId",
                table: "Files",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_ClientPFs_ClientId",
                table: "Files",
                column: "ClientId",
                principalTable: "ClientPFs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
