using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    // Data for the shared question table shown on
    // Challenge Details and on Update Challenge.
    public class QuestionOverviewViewModel
    {
        public int ChallengeId { get; set; }

        public IEnumerable<Question> Questions { get; set; }
            = new List<Question>();

        // Local URL the question pages return to after saving or cancelling.
        public string? ReturnUrl { get; set; }
    }
}
