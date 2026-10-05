namespace GamificationPlatform.ViewModels
{
    public class AchievementsViewModel
    {
        public string Username { get; set; } = string.Empty;

        public int TotalPoints { get; set; }

        public int MaxPoints { get; set; }

        public int UnlockedCount { get; set; }

        public int TotalAchievements { get; set; }

        public string MilestoneName { get; set; } = string.Empty;

        public string MilestoneIcon { get; set; } = string.Empty;

        public string MilestoneMessage { get; set; } = string.Empty;

        // Progress within the current milestone.
        public int ProgressPercent { get; set; }

        public int CurrentMilestonePoints { get; set; }

        public int NextMilestonePoints { get; set; }

        public List<AchievementItemViewModel> Achievements { get; set; }
            = new List<AchievementItemViewModel>();
    }
}