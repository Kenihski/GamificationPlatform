
using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or email is required.")]
        [StringLength(
            100,
            ErrorMessage = "Username or email cannot be longer than 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
