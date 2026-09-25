using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengeDetailsHistoryViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public List<ChallengeAttempt> Attempts { get; set; }
            = new List<ChallengeAttempt>();

        public int MaxPoints { get; set; }

        public int? BestAttemptId { get; set; }

        public int? LatestAttemptId { get; set; }
    }
}