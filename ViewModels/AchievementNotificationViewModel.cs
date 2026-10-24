namespace GamificationPlatform.ViewModels
{
    public class AchievementNotificationViewModel
    {
        public int UserAchievementId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Points { get; set; }

        public bool IsGoal { get; set; }
    }
}