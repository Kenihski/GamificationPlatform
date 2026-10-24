
using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.DAL
{
    public class UserImageRepository : IUserImageRepository
    {
        private readonly ChallengeDbContext _context;

        public UserImageRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        // Gets all images belonging to one user.
        public async Task<List<UserImage>> GetImagesByUserIdAsync(int userId)
        {
            return await _context.UserImages
                .Where(image => image.UserId == userId)
                .OrderByDescending(image => image.UserImageId)
                .ToListAsync();
        }

        // Gets all uploaded images (admin only).
        public async Task<List<UserImage>> GetAllImagesAsync()
        {
            return await _context.UserImages
                .Include(image => image.User)
                .OrderByDescending(image => image.UserImageId)
                .ToListAsync();
        }

        // Gets one image by its ID.
        public async Task<UserImage?> GetImageByIdAsync(int imageId)
        {
            return await _context.UserImages
                .FirstOrDefaultAsync(image => image.UserImageId == imageId);
        }

        // Saves information about an uploaded image.
        public async Task AddImageAsync(UserImage image)
        {
            _context.UserImages.Add(image);

            await _context.SaveChangesAsync();
        }

        // Deletes an image record from the database.
        public async Task DeleteImageAsync(UserImage image)
        {
            _context.UserImages.Remove(image);

            await _context.SaveChangesAsync();
        }
    }
}
