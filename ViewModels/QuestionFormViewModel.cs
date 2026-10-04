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

        public List<string> Options { get; set; }
            = new List<string>
            {
                "",
                "",
                "",
                ""
            };

        [Required(ErrorMessage = "Please select the correct answer.")]
        [Range(0, 3, ErrorMessage = "Please select one of the four answer fields.")]
        public int? CorrectOption { get; set; }

        [BindNever]
        public List<Challenge> Challenges { get; set; }
            = new List<Challenge>();
    }
}
