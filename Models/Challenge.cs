namespace GamificationPlatform.Models 
{ 
    public class Challenge 
    { 
        public int ChallengeId { get; set; } 
 
        public string Title { get; set; } = string.Empty; 
 
        public string Description { get; set; } = string.Empty; 
 
        public int MaxPoints { get; set; } 
 
        public string ImageUrl { get; set; } = string.Empty;

        // Time limit in minutes.
        // Null means unlimited time.
        public int? TimeLimitMinutes { get; set; }
 
        // False = Draft, True = Published. 
        public bool IsPublished { get; set; } = false; 
 
        // Foreign key: The user who created the challenge. 
        public int CreatedByUserId { get; set; } 
 
        // Navigation: Each challenge is created by one user. 
        public virtual User CreatedByUser { get; set; } = default!; 
 
        // Navigation: One challenge can have many questions. 
        public virtual List<Question> Questions { get; set; } 
            = new List<Question>(); 
 
        // Navigation: One challenge can be connected to many users. 
        public virtual List<UserChallenge> UserChallenges { get; set; } 
            = new List<UserChallenge>(); 
    } 
}