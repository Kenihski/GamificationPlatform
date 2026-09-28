using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamificationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndChallengeOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Challenges",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Challenges",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CreatedByUserId",
                table: "Challenges",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_CreatedByUserId",
                table: "Challenges",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_CreatedByUserId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_CreatedByUserId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Challenges");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Challenges",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
