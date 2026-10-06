namespace GamificationPlatform.Models
{
    public class AttemptAnswerOption
    {
        public int AttemptAnswerOptionId { get; set; }

        public int AttemptAnswerId { get; set; }

        public virtual AttemptAnswer AttemptAnswer { get; set; }
            = default!;

        public int QuestionOptionId { get; set; }

        public virtual QuestionOption QuestionOption { get; set; }
            = default!;
    }
}