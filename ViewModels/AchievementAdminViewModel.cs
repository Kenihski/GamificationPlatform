using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.ViewModels
{
    public class AchievementAdminViewModel
    {
        public int AchievementId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(
            100,
            ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(
            300,
            ErrorMessage = "Description cannot be longer than 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(
            5,
            20,
            ErrorMessage = "AP must be 5, 10, 15 or 20.")]
        public int Points { get; set; } = 5;

        [Required(ErrorMessage = "Achievement type is required.")]
        public string Type { get; set; } = string.Empty;

        [Range(
            1,
            1000,
            ErrorMessage = "Requirement must be between 1 and 1000.")]
        public int RequirementValue { get; set; }

        public bool IsActive { get; set; } = true;

        // Used to identify system-defined milestone goals.
        public bool IsGoal { get; set; }
    }
}