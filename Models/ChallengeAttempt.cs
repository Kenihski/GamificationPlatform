namespace GamificationPlatform.Models
{
    public class ChallengeAttempt
    {
        public int ChallengeAttemptId { get; set; }
        public int UserChallengeId { get; set; }
        // Navigation: Each attempt belongs to one user challenge.
        public virtual UserChallenge UserChallenge { get; set; } = default!;
        public int Score { get; set; }
        public bool Completed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}