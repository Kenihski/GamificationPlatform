namespace GamificationPlatform.ViewModels
{
    public class AchievementItemViewModel
    {
        public int AchievementId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Points { get; set; }

        public bool IsGoal { get; set; }

        public bool IsUnlocked { get; set; }

        public DateTime? UnlockedAt { get; set; }
    }
}