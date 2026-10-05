using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.Validation
{
    // Forms select bundled images; arbitrary URLs are not accepted.
    public sealed class AllowedImageAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || value is string text && string.IsNullOrWhiteSpace(text))
            {
                return true; // Required handles mandatory images separately.
            }

            return value is string image && Enumerable.Range(1, 13)
                .Any(number => image == $"/images/Question_{number}.png");
        }
    }
}
