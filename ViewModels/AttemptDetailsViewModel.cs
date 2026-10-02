namespace GamificationPlatform.ViewModels
{
    public class AttemptDetailsViewModel
    {
        public int ChallengeId { get; set; }

        public string ChallengeTitle { get; set; }
            = string.Empty;

        public int Score { get; set; }

        public int MaxPoints { get; set; }

        public string? TimeUsed { get; set; }

        public string? CompletedAt { get; set; }

        public List<AttemptAnswerViewModel> Answers { get; set; }
            = new List<AttemptAnswerViewModel>();
    }


    public class AttemptAnswerViewModel
    {
        public string QuestionTitle { get; set; }
            = string.Empty;

        public string QuestionDescription { get; set; }
            = string.Empty;

        public string? ImageUrl { get; set; }

        public int Points { get; set; }

        public int? SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public string? CorrectAnswer { get; set; }

        public List<AttemptOptionViewModel> Options { get; set; }
            = new List<AttemptOptionViewModel>();
    }


    public class AttemptOptionViewModel
    {
        public int QuestionOptionId { get; set; }

        public string Text { get; set; }
            = string.Empty;

        public bool IsSelected { get; set; }
    }
}