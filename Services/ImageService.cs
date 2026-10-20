
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Services
{
    public class ImageService : IImageService
    {
        private const long MaxFileSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private readonly IUserImageRepository _userImageRepository;
        private readonly ChallengeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImageService(
            IUserImageRepository userImageRepository,
            ChallengeDbContext context,
            IWebHostEnvironment environment)
        {
            _userImageRepository = userImageRepository;
            _context = context;
            _environment = environment;
        }

        // Gets images uploaded by the current user.
        public async Task<List<UserImage>> GetUserImagesAsync(int userId)
        {
            return await _userImageRepository
                .GetImagesByUserIdAsync(userId);
        }

        // Gets default images in the order used by the image selectors.
        public async Task<List<DefaultImage>> GetDefaultImagesAsync()
        {
            var images = await _context.DefaultImages.ToListAsync();

            return images
                .OrderBy(image =>
                    image.Name.StartsWith(
                        "Question ",
                        StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(image.Name.Substring(9), out _)
                        ? 1
                        : 0)
                .ThenBy(image =>
                    image.Name.StartsWith(
                        "Question ",
                        StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(image.Name.Substring(9), out int number)
                        ? number
                        : 0)
                .ThenBy(image => image.Name)
                .ToList();
        }

        // Allows default images and images owned by the current user.
        public async Task<bool> IsAllowedImageAsync(
            string? imageUrl,
            int userId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true;
            }

            bool isDefaultImage = await _context.DefaultImages
                .AnyAsync(image => image.ImageUrl == imageUrl);

            if (isDefaultImage)
            {
                return true;
            }

            var images = await _userImageRepository
                .GetImagesByUserIdAsync(userId);

            return images.Any(image => image.ImageUrl == imageUrl);
        }

        // Validates and saves an image to the user's image library.
        public async Task<ImageUploadResult> SaveImageAsync(
            IFormFile file,
            int userId)
        {
            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (file.Length == 0 ||
                file.Length > MaxFileSize ||
                !AllowedExtensions.Contains(extension))
            {
                return new ImageUploadResult(
                    null,
                    "Choose a JPG, PNG or WebP image up to 5 MB.");
            }

            byte[] header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                int count = 0;

                while (count < header.Length)
                {
                    int bytesRead = await stream.ReadAsync(
                        header.AsMemory(count));

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    count += bytesRead;
                }

                if (!IsValidImage(header, count, extension))
                {
                    return new ImageUploadResult(
                        null,
                        "The file does not match a supported image format.");
                }
            }

            string fileName = $"{Guid.NewGuid():N}{extension}";

            string folder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "images");

            Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, fileName);
            string imageUrl = $"/uploads/images/{fileName}";

            try
            {
                await using (var output =
                    new FileStream(fullPath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(output);
                }

                await _userImageRepository.AddImageAsync(
                    new UserImage
                    {
                        UserId = userId,
                        FileName = Path.GetFileName(file.FileName),
                        ImageUrl = imageUrl
                    });

                return new ImageUploadResult(imageUrl, null);
            }
            catch
            {
                // Remove the file if saving fails.
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                throw;
            }
        }

        // Checks whether the file signature matches its extension.
        private static bool IsValidImage(
            byte[] header,
            int count,
            string extension)
        {
            bool jpeg =
                count >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF;

            bool png =
                count >= 8 &&
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[]
                    {
                        137, 80, 78, 71, 13, 10, 26, 10
                    });

            bool webp =
                count >= 12 &&
                System.Text.Encoding.ASCII.GetString(
                    header, 0, 4) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(
                    header, 8, 4) == "WEBP";

            return extension switch
            {
                ".jpg" or ".jpeg" => jpeg,
                ".png" => png,
                ".webp" => webp,
                _ => false
            };
        }
    }
}
