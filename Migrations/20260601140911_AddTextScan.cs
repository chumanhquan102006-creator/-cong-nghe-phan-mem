using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddTextScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TextScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InputText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    OverallSimilarityScore = table.Column<double>(type: "float", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextScans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextScanMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextScanId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    SourceTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    InputSegment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchedSegment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SimilarityScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextScanMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextScanMatches_TextScans_TextScanId",
                        column: x => x.TextScanId,
                        principalTable: "TextScans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextScanMatches_TextScanId",
                table: "TextScanMatches",
                column: "TextScanId");

            migrationBuilder.CreateIndex(
                name: "IX_TextScans_UserId",
                table: "TextScans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextScanMatches");

            migrationBuilder.DropTable(
                name: "TextScans");
        }
    }
}
