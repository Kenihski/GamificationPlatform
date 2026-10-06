using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengeResultViewModel
    {
        public int ChallengeId { get; set; }

        public string ChallengeTitle { get; set; } = string.Empty;

        public int Score { get; set; }

        public int MaxScore { get; set; }

        // The total time used to complete the challenge.
        public TimeSpan TimeUsed { get; set; }

        public List<QuestionResultViewModel> QuestionResults { get; set; }
            = new List<QuestionResultViewModel>();
    }

    public class QuestionResultViewModel
    {
        public Question Question { get; set; } = default!;

        // Stores all selected option IDs for choice questions.
        public List<int> SelectedOptionIds { get; set; }
            = new List<int>();

        // Stores the submitted answer for Short Answer questions.
        public string? TextAnswer { get; set; }

        public bool IsCorrect { get; set; }
    }
}