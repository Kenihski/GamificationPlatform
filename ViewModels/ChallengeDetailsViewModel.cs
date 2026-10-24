using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ChallengeDetailsHistoryViewModel
    {
        public Challenge Challenge { get; set; } = default!;

        public List<ChallengeAttemptHistoryViewModel> Attempts
            { get; set; }
            = new List<ChallengeAttemptHistoryViewModel>();

        public int MaxPoints { get; set; }

        // Determines whether the current user can
        // manage the challenge and its questions.
        public bool CanManage { get; set; }

        // Determines whether the question section
        // should be open when the page loads.
        public bool ShowQuestions { get; set; }
    }


    // Contains the attempt information
    // needed by the challenge details view.
    public class ChallengeAttemptHistoryViewModel
    {
        public int ChallengeAttemptId { get; set; }

        public int AttemptNumber { get; set; }

        public int Score { get; set; }

        public string? TimeUsed { get; set; }

        public string? CompletedAt { get; set; }

        public bool IsBest { get; set; }

        public bool IsLatest { get; set; }
    }
}