using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class ManageImagesViewModel
    {
        public List<UserImage> UserImages { get; set; } = new();

        public List<DefaultImage> DefaultImages { get; set; } = new();
    }
}