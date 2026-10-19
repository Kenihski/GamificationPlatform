
using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    // Contains the information displayed for a featured challenge.
    public class FeaturedChallengeViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public int MaxPoints { get; set; }

        public string CreatorUsername { get; set; } = string.Empty;

        public bool IsPopular { get; set; }
    }
}
