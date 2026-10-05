using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengesViewModel
    {
        public IEnumerable<Challenge> Challenges { get; set; }

        public string? CurrentViewName { get; set; }

        // Stores the selected challenge type filter.
        public string CurrentFilter { get; set; }

        // Stores the IDs of challenges completed
        // by the logged-in user.
        public HashSet<int> CompletedChallengeIds { get; set; }

        public ChallengesViewModel(
            IEnumerable<Challenge> challenges,
            string? currentViewName,
            string currentFilter = "All",
            IEnumerable<int>? completedChallengeIds = null)
        {
            Challenges = challenges;
            CurrentViewName = currentViewName;
            CurrentFilter = currentFilter;

            CompletedChallengeIds =
                completedChallengeIds?.ToHashSet()
                ?? new HashSet<int>();
        }
    }
}