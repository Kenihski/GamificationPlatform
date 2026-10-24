
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public interface IUserImageRepository
    {
        Task<List<UserImage>> GetImagesByUserIdAsync(int userId);

        Task<List<UserImage>> GetAllImagesAsync();

        Task<UserImage?> GetImageByIdAsync(int imageId);

        Task AddImageAsync(UserImage image);

        Task DeleteImageAsync(UserImage image);
    }
}
