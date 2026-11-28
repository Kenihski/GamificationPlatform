using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class TakeChallengeViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public int ChallengeAttemptId { get; set; }

        // The time when the challenge attempt started.
        public DateTime StartedAt { get; set; }

        // True when the user opened the challenge from My Challenges,
        // so "Back to Challenge" keeps the "Back to My Challenges" link.
        public bool FromMyChallenges { get; set; }
    }
}