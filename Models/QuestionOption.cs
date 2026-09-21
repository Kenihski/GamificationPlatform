namespace GamificationPlatform.Models
{
    public class QuestionOption
    {
        public int QuestionOptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
        // Navigation: Each answer option belongs to one question.
        public virtual Question Question { get; set; } = default!;
    }
}