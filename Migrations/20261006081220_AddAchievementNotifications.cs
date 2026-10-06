using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamificationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationSeen",
                table: "UserAchievements",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationSeen",
                table: "UserAchievements");
        }
    }
}
