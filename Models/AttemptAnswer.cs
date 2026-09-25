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

        // Null means the question was not answered
        public int? SelectedOptionId { get; set; }

        public virtual QuestionOption? SelectedOption { get; set; }
    }
}