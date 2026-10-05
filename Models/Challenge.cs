using System.ComponentModel.DataAnnotations;
using GamificationPlatform.Validation;

namespace GamificationPlatform.Models 
{ 
    public class Challenge 
    { 
        public int ChallengeId { get; set; } 
 
        [Required(ErrorMessage = "Challenge title is required.")]
        [StringLength(
            100,
            ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(
            500,
            ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; } = string.Empty; 

        public int MaxPoints { get; set; } 

        [Required(ErrorMessage = "Please select an image.")]
        [AllowedImage(ErrorMessage = "Please select one of the available images.")]
        public string ImageUrl { get; set; } = string.Empty;

        // Time limit in minutes.
        // Null means unlimited time.
        [AllowedTimeLimit(ErrorMessage = "Choose unlimited or a time limit of 10, 20, 30, 60, 90, or 120 minutes.")]
        public int? TimeLimitMinutes { get; set; }

        // False = Draft, True = Published. 
        public bool IsPublished { get; set; } = false; 

        // False = Community, True = Core.
        // New challenges are Community by default.
        public bool IsCore { get; set; } = false;

        // Foreign key: The user who created the challenge. 
        public int CreatedByUserId { get; set; } 

        // Navigation: Each challenge is created by one user. 
        public virtual User CreatedByUser { get; set; } = default!; 

        // Navigation: One challenge can have many questions. 
        public virtual List<Question> Questions { get; set; } 
            = new List<Question>(); 
        
        // Navigation: One challenge can be connected to many users. 
        public virtual List<UserChallenge> UserChallenges { get; set; } 
            = new List<UserChallenge>(); 
    } 
}
