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
    }
}

