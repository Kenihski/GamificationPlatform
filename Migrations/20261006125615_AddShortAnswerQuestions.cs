using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamificationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddShortAnswerQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionAcceptedAnswers",
                columns: table => new
                {
                    QuestionAcceptedAnswerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAcceptedAnswers", x => x.QuestionAcceptedAnswerId);
                    table.ForeignKey(
                        name: "FK_QuestionAcceptedAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAcceptedAnswers_QuestionId",
                table: "QuestionAcceptedAnswers",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAcceptedAnswers");
        }
    }
}
