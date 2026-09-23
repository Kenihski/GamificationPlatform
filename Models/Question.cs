namespace GamificationPlatform.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Points { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int ChallengeId { get; set; }
        // Navigation: Each question belongs to one challenge.
        public virtual Challenge Challenge { get; set; } = default!;
        // Navigation: A question can have multiple answer options.
        public virtual List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}