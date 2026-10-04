using System.ComponentModel.DataAnnotations;
using GamificationPlatform.Validation;
namespace GamificationPlatform.Models
{
    public class Question
    {   
        public int QuestionId { get; set; }
        [Required(ErrorMessage = "Question title is required.")]
        [StringLength(
            150,
            ErrorMessage = "Question title cannot be longer than 150 characters.")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Question description is required.")]
        [StringLength(
            500,
            ErrorMessage = "Question description cannot be longer than 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(
            1,
            100,
            ErrorMessage = "Points must be between 1 and 100.")]
        public int Points { get; set; }
        [AllowedImage(ErrorMessage = "Please select one of the available images.")]
        public string? ImageUrl { get; set; }
        public int ChallengeId { get; set; }
        // Navigation: Each question belongs to one challenge.
        public virtual Challenge Challenge { get; set; } = default!;
        // Navigation: A question can have multiple answer options.
        public virtual List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
