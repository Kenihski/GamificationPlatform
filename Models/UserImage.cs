
namespace GamificationPlatform.Models
{
    public class UserImage
    {
        public int UserImageId { get; set; }

        public int UserId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        // Navigation: Each image belongs to one user.
        public virtual User User { get; set; } = null!;
    }
}
