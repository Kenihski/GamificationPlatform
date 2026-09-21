namespace GamificationPlatform.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Navigation: A user can participate in multiple challenges.
        public virtual List<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    }
}