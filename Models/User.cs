
using Microsoft.AspNetCore.Identity;

namespace GamificationPlatform.Models
{
    public class User : IdentityUser<int>
    {
        // Navigation: One user can participate in many challenges.
        public virtual List<UserChallenge> UserChallenges { get; set; }
            = new List<UserChallenge>();

        // Navigation: One user can create many challenges.
        public virtual List<Challenge> CreatedChallenges { get; set; }
            = new List<Challenge>();

        // Navigation: One user can unlock many achievements.
        public virtual List<UserAchievement> UserAchievements { get; set; }
            = new List<UserAchievement>();

        // Navigation: One user can upload many images.
        public virtual List<UserImage> UserImages { get; set; }
            = new List<UserImage>();
    }
}
