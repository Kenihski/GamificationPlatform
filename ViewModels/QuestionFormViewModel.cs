using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    // Used by both the Create and Update
    // forms for questions.
    public class QuestionFormViewModel
    {
        [Required]
        public Question Question { get; set; }
            = new Question();

        // Used for Single Choice and Multiple Choice.
        public List<string> Options { get; set; }
            = new List<string>
            {
                "",
                "",
                "",
                ""
            };

        // Stores the indexes of the correct answer options.
        public List<int> CorrectOptions { get; set; }
            = new List<int>();

        // Used for Short Answer questions.
        // The creator can define several accepted answers.
        public List<string> AcceptedAnswers { get; set; }
            = new List<string>
            {
                "",
                "",
                "",
                ""
            };

        [BindNever]
        public List<Challenge> Challenges { get; set; }
            = new List<Challenge>();
    }
}