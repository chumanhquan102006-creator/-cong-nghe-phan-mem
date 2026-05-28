using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddSimilarityChecker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimilarityChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EssayId = table.Column<int>(type: "int", nullable: false),
                    OverallSimilarityScore = table.Column<double>(type: "float", nullable: false),
                    TotalDocumentsCompared = table.Column<int>(type: "int", nullable: false),
                    TotalMatches = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimilarityChecks_Essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "Essays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimilarityMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SimilarityCheckId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    EssaySegment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SimilarityScore = table.Column<double>(type: "float", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimilarityMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimilarityMatches_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SimilarityMatches_SimilarityChecks_SimilarityCheckId",
                        column: x => x.SimilarityCheckId,
                        principalTable: "SimilarityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimilarityChecks_EssayId",
                table: "SimilarityChecks",
                column: "EssayId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarityMatches_DocumentId",
                table: "SimilarityMatches",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SimilarityMatches_SimilarityCheckId",
                table: "SimilarityMatches",
                column: "SimilarityCheckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimilarityMatches");

            migrationBuilder.DropTable(
                name: "SimilarityChecks");
        }
    }
}
