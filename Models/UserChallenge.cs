namespace GamificationPlatform.Models
{
    public class UserChallenge
    {
        public int UserChallengeId { get; set; }
        public int UserId { get; set; }
        // Navigation: Each UserChallenge belongs to one user.
        public virtual User User { get; set; } = default!;
        public int ChallengeId { get; set; }
        // Navigation: Each UserChallenge belongs to one challenge.
        public virtual Challenge Challenge { get; set; } = default!;
        // Navigation: A user can have multiple attempts at the same challenge.
        public virtual List<ChallengeAttempt> Attempts { get; set; } = new List<ChallengeAttempt>();
    }
}