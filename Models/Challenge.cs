using System;

namespace GamificationPlatform.Models
{
    public class Challenge
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxPoints { get; set; }
        public string? ImageUrl { get; set; }

         // Navigation: A challenge can contain multiple questions.
        public virtual List<Question> Questions { get; set; } = new List<Question>();

        // Navigation: A challenge can be associated with multiple users.
        public virtual List<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();

    }
}

