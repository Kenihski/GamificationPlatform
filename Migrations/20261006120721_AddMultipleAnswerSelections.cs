using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamificationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleAnswerSelections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttemptAnswers_QuestionOptions_SelectedOptionId",
                table: "AttemptAnswers");

            migrationBuilder.DropIndex(
                name: "IX_AttemptAnswers_SelectedOptionId",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "SelectedOptionId",
                table: "AttemptAnswers");

            migrationBuilder.AddColumn<string>(
                name: "TextAnswer",
                table: "AttemptAnswers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttemptAnswerOptions",
                columns: table => new
                {
                    AttemptAnswerOptionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttemptAnswerId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionOptionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptAnswerOptions", x => x.AttemptAnswerOptionId);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerOptions_AttemptAnswers_AttemptAnswerId",
                        column: x => x.AttemptAnswerId,
                        principalTable: "AttemptAnswers",
                        principalColumn: "AttemptAnswerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerOptions_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "QuestionOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerOptions_AttemptAnswerId_QuestionOptionId",
                table: "AttemptAnswerOptions",
                columns: new[] { "AttemptAnswerId", "QuestionOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerOptions_QuestionOptionId",
                table: "AttemptAnswerOptions",
                column: "QuestionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttemptAnswerOptions");

            migrationBuilder.DropColumn(
                name: "TextAnswer",
                table: "AttemptAnswers");

            migrationBuilder.AddColumn<int>(
                name: "SelectedOptionId",
                table: "AttemptAnswers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswers_SelectedOptionId",
                table: "AttemptAnswers",
                column: "SelectedOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttemptAnswers_QuestionOptions_SelectedOptionId",
                table: "AttemptAnswers",
                column: "SelectedOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "QuestionOptionId");
        }
    }
}
