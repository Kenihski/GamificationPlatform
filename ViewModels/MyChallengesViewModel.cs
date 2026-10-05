namespace GamificationPlatform.ViewModels
{
    public class MyChallengesViewModel
    {
        public List<MyChallengeViewModel> Challenges { get; set; }
            = new List<MyChallengeViewModel>();
    }

    public class MyChallengeViewModel
    {
        public int ChallengeId { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public int QuestionCount { get; set; }

        public int MaxPoints { get; set; }

        public bool IsPublished { get; set; }

        // False = Community, True = Core.
        public bool IsCore { get; set; }
    }
}