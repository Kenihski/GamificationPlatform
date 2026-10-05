namespace GamificationPlatform.Models
{
    public class Achievement
    {
        public int AchievementId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Achievement Points awarded when unlocked.
        public int Points { get; set; }

        // Defines what kind of requirement this achievement uses.
        public string Type { get; set; } = string.Empty;

        // The value required to unlock the achievement.
        public int RequirementValue { get; set; }

        // Inactive achievements are not available to unlock.
        public bool IsActive { get; set; } = true;

        // Goals are milestone achievements that do not give AP.
        public bool IsGoal { get; set; } = false;

        // Navigation: One achievement can be unlocked by many users.
        public virtual List<UserAchievement> UserAchievements { get; set; }
            = new List<UserAchievement>();
    }
}