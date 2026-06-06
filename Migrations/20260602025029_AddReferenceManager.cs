using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicAIAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferenceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalOrPublisher = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    WebsiteName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Doi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Volume = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Issue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pages = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApaInTextCitation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApaReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MlaInTextCitation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MlaReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferenceItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceItems_UserId",
                table: "ReferenceItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferenceItems");
        }
    }
}
