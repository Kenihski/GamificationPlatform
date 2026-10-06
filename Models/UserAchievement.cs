namespace GamificationPlatform.Models
{
    public class UserAchievement
    {
        public int UserAchievementId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; } = default!;

        public int AchievementId { get; set; }

        public virtual Achievement Achievement { get; set; } = default!;

        // Stores when the achievement was unlocked.
        public DateTime UnlockedAt { get; set; }

        // Tracks whether the user has seen the
        // achievement unlock notification.
        public bool NotificationSeen { get; set; } = false;
    }
}