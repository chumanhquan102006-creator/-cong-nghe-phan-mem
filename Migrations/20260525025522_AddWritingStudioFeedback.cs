using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingStudioFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EssayType",
                table: "Essays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "Essays",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FeedbackReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EssayId = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    GrammarFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcademicToneFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThesisFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StructureFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogicFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitationFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneralSuggestions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackReports_Essays_EssayId",
                        column: x => x.EssayId,
                        principalTable: "Essays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReports_EssayId",
                table: "FeedbackReports",
                column: "EssayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackReports");

            migrationBuilder.DropColumn(
                name: "EssayType",
                table: "Essays");

            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "Essays");
        }
    }
}
