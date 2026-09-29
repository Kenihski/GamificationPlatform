using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class UserDetailsViewModel
    {
        public User User { get; set; } = default!;

        // Challenges created by this user.
        public List<Challenge> CreatedChallenges { get; set; }
            = new List<Challenge>();

        // Challenges this user has taken.
        public List<UserChallenge> ChallengeHistory { get; set; }
            = new List<UserChallenge>();
    }
}