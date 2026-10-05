namespace GamificationPlatform.ViewModels
{
    public class HallOfFameViewModel
    {
        public List<HallOfFameUserViewModel> Users { get; set; }
            = new List<HallOfFameUserViewModel>();
    }

    public class HallOfFameUserViewModel
    {
        public int Rank { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string MilestoneName { get; set; } = string.Empty;

        public string MilestoneIcon { get; set; } = string.Empty;

        public int AchievementCount { get; set; }

        public int AchievementPoints { get; set; }
    }
}