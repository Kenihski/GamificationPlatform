namespace GamificationPlatform.ViewModels
{
    public class TotalLeaderboardViewModel
    {
        public List<TotalLeaderboardEntryViewModel> Entries { get; set; }
            = new();
    }

    public class TotalLeaderboardEntryViewModel
    {
        public int Rank { get; set; }

        public string Username { get; set; }
            = string.Empty;

        public double TotalPoints { get; set; }

        public int CoreChallengesCompleted { get; set; }

        // Total time used on the Core Challenges
        // that count toward the leaderboard.
        public TimeSpan TotalTime { get; set; }
    }
}