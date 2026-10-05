namespace GamificationPlatform.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        // True if the user is an admin.
        public bool IsAdmin { get; set; }

        // Navigation: One user can participate in many challenges.
        public virtual List<UserChallenge> UserChallenges { get; set; }
            = new List<UserChallenge>();

        // Navigation: One user can create many challenges.
        public virtual List<Challenge> CreatedChallenges { get; set; }
            = new List<Challenge>();

        // Navigation: One user can unlock many achievements.
        public virtual List<UserAchievement> UserAchievements { get; set; }
            = new List<UserAchievement>();
    }
}