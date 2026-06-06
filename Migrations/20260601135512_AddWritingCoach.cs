using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingCoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WritingCoachSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EssayType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThesisStatement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AIResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingCoachSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WritingCoachSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WritingCoachSessions_UserId",
                table: "WritingCoachSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WritingCoachSessions");
        }
    }
}
