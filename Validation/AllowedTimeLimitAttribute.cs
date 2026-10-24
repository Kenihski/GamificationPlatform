using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.Validation
{
    // Keep server validation consistent with the challenge form dropdown.
    public sealed class AllowedTimeLimitAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value == null || value is int minutes &&
                new[] { 10, 20, 30, 60, 90, 120 }.Contains(minutes);
        }
    }
}
