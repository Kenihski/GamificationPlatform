namespace GamificationPlatform.Models
{
    public class AttemptAnswer
    {
        public int AttemptAnswerId { get; set; }

        public int ChallengeAttemptId { get; set; }

        public virtual ChallengeAttempt ChallengeAttempt { get; set; }
            = default!;

        public int QuestionId { get; set; }

        public virtual Question Question { get; set; }
            = default!;

        // Stores the selected options for choice questions.
        public virtual List<AttemptAnswerOption> SelectedOptions { get; set; }
            = new List<AttemptAnswerOption>();

        // Used for short-answer questions.
        public string? TextAnswer { get; set; }
    }
}