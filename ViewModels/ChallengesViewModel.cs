using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengesViewModel
    {
        public IEnumerable<Challenge> Challenges { get; set; }
        public string? CurrentViewName { get; set; }

        public ChallengesViewModel(
            IEnumerable<Challenge> challenges,
            string? currentViewName)
        {
            Challenges = challenges;
            CurrentViewName = currentViewName;
        }
    }
}