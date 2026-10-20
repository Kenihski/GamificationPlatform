
using GamificationPlatform.Models;
using Microsoft.AspNetCore.Http;

namespace GamificationPlatform.Services
{
    public interface IImageService
    {
        Task<List<UserImage>> GetUserImagesAsync(int userId);

        Task<List<DefaultImage>> GetDefaultImagesAsync();

        Task<bool> IsAllowedImageAsync(string? imageUrl, int userId);

        Task<ImageUploadResult> SaveImageAsync(
            IFormFile file,
            int userId);
    }

    public record ImageUploadResult(
        string? ImageUrl,
        string? ErrorMessage)
    {
        public bool IsSuccess => ErrorMessage == null;
    }
}
