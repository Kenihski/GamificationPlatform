using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class UserDetailsViewModel
    {
        public User User { get; set; } = default!;

        // Challenges created by this user.
        public List<Challenge> CreatedChallenges { get; set; }
            = new List<Challenge>();

        // Challenge history prepared for the view.
        public List<UserChallengeHistoryViewModel> ChallengeHistory
            { get; set; }
            = new List<UserChallengeHistoryViewModel>();
    }


    public class UserChallengeHistoryViewModel
    {
        public string ChallengeTitle { get; set; }
            = string.Empty;

        public int MaxPoints { get; set; }

        public List<UserAttemptHistoryViewModel> Attempts
            { get; set; }
            = new List<UserAttemptHistoryViewModel>();
    }


    public class UserAttemptHistoryViewModel
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