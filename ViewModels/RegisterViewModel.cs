using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(
            ErrorMessage = "Please enter a valid email address.")]
        [StringLength(
            100,
            ErrorMessage = "Email cannot be longer than 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;


        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Username) && Username.Trim().Length < 3)
            {
                yield return new ValidationResult(
                    "Username must be between 3 and 50 characters.",
                    new[] { nameof(Username) });
            }

            if (!string.IsNullOrWhiteSpace(Username) &&
                !string.IsNullOrWhiteSpace(Password) &&
                Username.Trim().Equals(
                    Password,
                    StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Username and password cannot be the same.",
                    new[] { nameof(Password) });
            }
        }
    }
}
