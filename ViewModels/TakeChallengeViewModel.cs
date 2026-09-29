using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class TakeChallengeViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public int ChallengeAttemptId { get; set; }

        // The time when the challenge attempt started.
        public DateTime StartedAt { get; set; }
    }
}