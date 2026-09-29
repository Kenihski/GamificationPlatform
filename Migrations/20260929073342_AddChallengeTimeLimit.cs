using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamificationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeTimeLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "Challenges",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "Challenges");
        }
    }
}
