using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class LeaderboardViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public int MaxPoints { get; set; }

        public List<LeaderboardEntryViewModel> Entries { get; set; }
            = new List<LeaderboardEntryViewModel>();
    }

    public class LeaderboardEntryViewModel
    {
        public int Rank { get; set; }

        public string Username { get; set; } = string.Empty;

        public int Score { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TimeSpan? TimeUsed =>
            CompletedAt.HasValue
                ? CompletedAt.Value - StartedAt
                : null;
    }
}