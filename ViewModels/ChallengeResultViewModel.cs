using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengeResultViewModel
    {
        public string ChallengeTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; }
            = new List<QuestionResultViewModel>();
    }

    public class QuestionResultViewModel
    {
        public Question Question { get; set; } = default!;
        public int? SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
    }
}