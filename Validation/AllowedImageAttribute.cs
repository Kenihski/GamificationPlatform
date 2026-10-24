
using System.ComponentModel.DataAnnotations;

namespace GamificationPlatform.Validation
{
    // Only images stored in approved folders are accepted.
    public sealed class AllowedImageAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // No image is allowed.
            if (value == null ||
                value is string text && string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (value is not string image)
            {
                return false;
            }

            // Existing default images.
            bool isDefaultImage = Enumerable.Range(1, 13)
                .Any(number => image == $"/images/Question_{number}.png");

            // Uploaded images use a GUID filename.
            bool isUploadedImage =
                System.Text.RegularExpressions.Regex.IsMatch(
                    image,
                    @"^/uploads/images/[a-fA-F0-9]{32}\.(jpg|jpeg|png|webp)$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return isDefaultImage || isUploadedImage;
        }
    }
}
