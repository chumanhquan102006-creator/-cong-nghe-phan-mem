using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddCitationChecker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CitationChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EssayId = table.Column<int>(type: "int", nullable: false),
                    TotalInTextCitations = table.Column<int>(type: "int", nullable: false),
                    TotalReferences = table.Column<int>(type: "int", nullable: false),
                    MissingReferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnusedReferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormatIssues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationChecks_Essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "Essays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitationChecks_EssayId",
                table: "CitationChecks",
                column: "EssayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitationChecks");
        }
    }
}
