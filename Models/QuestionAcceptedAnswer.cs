namespace GamificationPlatform.Models
{
    public class QuestionAcceptedAnswer
    {
        public int QuestionAcceptedAnswerId { get; set; }

        public int QuestionId { get; set; }

        public virtual Question Question { get; set; } = default!;

        public string Text { get; set; } = string.Empty;
    }
}