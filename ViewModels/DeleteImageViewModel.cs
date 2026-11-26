using GamificationPlatform.Models;

namespace GamificationPlatform.ViewModels
{
    public class DeleteImageViewModel
    {
        public int ImageId { get; set; }
        public string ImageName { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public bool IsDefaultImage { get; set; }
        public string ReturnUrl { get; set; } = "";
        public List<Challenge> Challenges { get; set; } = new();
    }
}
