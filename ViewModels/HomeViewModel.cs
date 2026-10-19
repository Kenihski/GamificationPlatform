
using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class HomeViewModel
    {
        // Challenges displayed in the featured carousel.
        public List<FeaturedChallengeViewModel> FeaturedChallenges { get; set; }
            = new List<FeaturedChallengeViewModel>();

        // Top three users from the Core Leaderboard.
        public List<TotalLeaderboardEntryViewModel> CoreLeaderboardEntries { get; set; }
            = new List<TotalLeaderboardEntryViewModel>();

        // Top three users from the Hall of Fame.
        public List<HallOfFameUserViewModel> HallOfFameEntries { get; set; }
            = new List<HallOfFameUserViewModel>();
    }
}
