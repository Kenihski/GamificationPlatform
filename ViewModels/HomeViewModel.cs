
using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class HomeViewModel
    {
        // The challenge selected as Challenge of the Day.
        public Challenge? ChallengeOfTheDay { get; set; }

        // Maximum points for Challenge of the Day.
        public int MaxPoints { get; set; }

        // Top three users from the Core Leaderboard.
        public List<TotalLeaderboardEntryViewModel> CoreLeaderboardEntries { get; set; }
            = new List<TotalLeaderboardEntryViewModel>();

        // Top three users from the Hall of Fame.
        public List<HallOfFameUserViewModel> HallOfFameEntries { get; set; }
            = new List<HallOfFameUserViewModel>();
    }
}
