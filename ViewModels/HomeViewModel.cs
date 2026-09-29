using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class HomeViewModel
    {
        // The challenge selected as Challenge of the Day.
        public Challenge? ChallengeOfTheDay { get; set; }

        // Maximum points for Challenge of the Day.
        public int MaxPoints { get; set; }

        // Top leaderboard entries for Challenge of the Day.
        public List<LeaderboardEntryViewModel> LeaderboardEntries { get; set; }
            = new List<LeaderboardEntryViewModel>();
    }
}