using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class CreateQuestionViewModel
    {
        public Question Question { get; set; } = new Question();

        public List<string> Options { get; set; } = new List<string>
        {
            "",
            "",
            "",
            ""
        };

        public int CorrectOption { get; set; }

        public List<Challenge> Challenges { get; set; }
            = new List<Challenge>();
    }
}