
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamificationPlatform.Controllers
{
    [Authorize]
    public class UserImageController : Controller
    {
        private readonly IUserImageRepository _imageRepository;
        private readonly ChallengeDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserImageController> _logger;

        private const long MaxFileSize = 5 * 1024 * 1024;

        public UserImageController(
            IUserImageRepository imageRepository,
            ChallengeDbContext context,
            IWebHostEnvironment environment,
            ILogger<UserImageController> logger)
        {
            _imageRepository = imageRepository;
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // Shows images belonging to the logged-in user.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var images = await _imageRepository
                .GetImagesByUserIdAsync(userId);

            return View(images);
        }

        // Uploads a new user image.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > MaxFileSize)
            {
                TempData["ErrorMessage"] =
                    "The image must be smaller than 5 MB.";

                return RedirectToAction(nameof(Index));
            }

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] =
                    "Only JPG, PNG and WebP images are allowed.";

                return RedirectToAction(nameof(Index));
            }

            byte[] header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                int bytesRead = await stream.ReadAsync(header);

                if (bytesRead < 12 || !IsValidImage(header, extension))
                {
                    TempData["ErrorMessage"] =
                        "The selected file is not a valid image.";

                    return RedirectToAction(nameof(Index));
                }
            }

            int userId = GetCurrentUserId();

            string uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "images");

            Directory.CreateDirectory(uploadFolder);

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string filePath = Path.Combine(uploadFolder, fileName);

            try
            {
                using (var stream = new FileStream(
                    filePath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                var image = new UserImage
                {
                    UserId = userId,
                    FileName = Path.GetFileName(file.FileName),
                    ImageUrl = "/uploads/images/" + fileName
                };

                await _imageRepository.AddImageAsync(image);

                _logger.LogInformation(
                    "User {UserId} uploaded image {ImageId}.",
                    userId,
                    image.UserImageId);

                TempData["SuccessMessage"] =
                    "Image uploaded successfully.";
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _logger.LogError(ex, "Image upload failed.");

                TempData["ErrorMessage"] =
                    "Could not upload the image.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Deletes an uploaded user image.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? returnUrl)
        {
            var image = await _imageRepository.GetImageByIdAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            int userId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            if (image.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            try
            {
                var challenges = await _context.Challenges
                    .Where(c => c.ImageUrl == image.ImageUrl)
                    .ToListAsync();

                var questions = await _context.Questions
                    .Where(q => q.ImageUrl == image.ImageUrl)
                    .ToListAsync();

                foreach (var challenge in challenges)
                {
                    challenge.ImageUrl = "";
                }

                foreach (var question in questions)
                {
                    question.ImageUrl = "";
                }

                await _context.SaveChangesAsync();
                await _imageRepository.DeleteImageAsync(image);

                string fileName = Path.GetFileName(image.ImageUrl);

                string filePath = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "images",
                    fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _logger.LogInformation(
                    "User {UserId} deleted image {ImageId}.",
                    userId,
                    id);

                TempData["SuccessMessage"] =
                    "Image deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete image {ImageId}.",
                    id);

                TempData["ErrorMessage"] =
                    "Could not delete the image.";
            }

            return RedirectAfterDelete(returnUrl);
        }

        private IActionResult RedirectAfterDelete(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(
                nameof(MyChallengesRedirect),
                "UserImage");
        }

        [HttpGet]
        public IActionResult MyChallengesRedirect()
        {
            return RedirectToAction(
                "MyChallenges",
                "Challenge");
        }

        // Shows uploaded and default images to administrators.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            await RegisterExistingDefaultImagesAsync();

            // Populate filenames for existing images after migration.
            await UpdateMissingDefaultFileNamesAsync();

            var defaultImages = await _context.DefaultImages
                .ToListAsync();

            var viewModel = new ManageImagesViewModel
            {
                UserImages = await _imageRepository.GetAllImagesAsync(),

                // Topic images first, Question images last.
                DefaultImages = defaultImages
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
                    .ToList()
            };

            return View(viewModel);
        }

        // Uploads a default image available to all users.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadDefaultImage(
            string name,
            IFormFile? file)
        {
            name = name?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            {
                TempData["ErrorMessage"] =
                    "Image name must contain between 1 and 100 characters.";

                return RedirectToAction(nameof(Admin));
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction(nameof(Admin));
            }

            if (file.Length > MaxFileSize)
            {
                TempData["ErrorMessage"] =
                    "The image must be smaller than 5 MB.";

                return RedirectToAction(nameof(Admin));
            }

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] =
                    "Only JPG, PNG and WebP images are allowed.";

                return RedirectToAction(nameof(Admin));
            }

            byte[] header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                int bytesRead = await stream.ReadAsync(header);

                if (bytesRead < 12 || !IsValidImage(header, extension))
                {
                    TempData["ErrorMessage"] =
                        "The selected file is not a valid image.";

                    return RedirectToAction(nameof(Admin));
                }
            }

            string folder = Path.Combine(
                _environment.WebRootPath,
                "images");

            Directory.CreateDirectory(folder);

            // Unique filename used for physical storage.
            string storedFileName =
                Guid.NewGuid().ToString("N") + extension;

            string filePath = Path.Combine(folder, storedFileName);

            // Original filename displayed to administrators.
            string originalFileName =
                Path.GetFileName(file.FileName);

            try
            {
                using (var stream = new FileStream(
                    filePath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                var image = new DefaultImage
                {
                    Name = name,
                    FileName = originalFileName,
                    ImageUrl = "/images/" + storedFileName
                };

                _context.DefaultImages.Add(image);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Default image uploaded successfully.";
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _logger.LogError(ex, "Default image upload failed.");

                TempData["ErrorMessage"] =
                    "Could not upload the default image.";
            }

            return RedirectToAction(nameof(Admin));
        }

        // Changes the display name of a default image.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RenameDefaultImage(
            int id,
            string name)
        {
            var image = await _context.DefaultImages.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            name = name?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            {
                TempData["ErrorMessage"] =
                    "Image name must contain between 1 and 100 characters.";

                return RedirectToAction(nameof(Admin));
            }

            image.Name = name;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Default image renamed successfully.";

            return RedirectToAction(nameof(Admin));
        }

        // Deletes a default image and clears its references.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDefaultImage(int id)
        {
            var image = await _context.DefaultImages.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            try
            {
                var challenges = await _context.Challenges
                    .Where(c => c.ImageUrl == image.ImageUrl)
                    .ToListAsync();

                var questions = await _context.Questions
                    .Where(q => q.ImageUrl == image.ImageUrl)
                    .ToListAsync();

                foreach (var challenge in challenges)
                {
                    challenge.ImageUrl = "";
                }

                foreach (var question in questions)
                {
                    question.ImageUrl = "";
                }

                _context.DefaultImages.Remove(image);

                await _context.SaveChangesAsync();

                // Delete using the stored filename, not FileName.
                string storedFileName =
                    Path.GetFileName(image.ImageUrl);

                string filePath = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    storedFileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                TempData["SuccessMessage"] =
                    "Default image deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete default image {ImageId}.",
                    id);

                TempData["ErrorMessage"] =
                    "Could not delete the default image.";
            }

            return RedirectToAction(nameof(Admin));
        }

        // Registers bundled Question images if the table is empty.
        private async Task RegisterExistingDefaultImagesAsync()
        {
            if (await _context.DefaultImages.AnyAsync())
            {
                return;
            }

            var defaultImages = new List<DefaultImage>();

            for (int number = 1; number <= 7; number++)
            {
                string fileName = $"Question_{number}.png";
                string imageUrl = "/images/" + fileName;

                string filePath = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    fileName);

                if (System.IO.File.Exists(filePath))
                {
                    defaultImages.Add(new DefaultImage
                    {
                        Name = $"Question {number}",
                        FileName = fileName,
                        ImageUrl = imageUrl
                    });
                }
            }

            if (defaultImages.Count > 0)
            {
                _context.DefaultImages.AddRange(defaultImages);
                await _context.SaveChangesAsync();
            }
        }

        // Fills in missing filenames for images already in the database.
        private async Task UpdateMissingDefaultFileNamesAsync()
        {
            var images = await _context.DefaultImages
                .Where(image => image.FileName == "" ||
                                image.FileName == null)
                .ToListAsync();

            if (images.Count == 0)
            {
                return;
            }

            foreach (var image in images)
            {
                // For existing bundled images, the stored filename
                // is also the original filename.
                // For older GUID uploads, the original filename
                // cannot be recovered from the stored URL.
                image.FileName = Path.GetFileName(image.ImageUrl);
            }

            await _context.SaveChangesAsync();
        }

        private int GetCurrentUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private static bool IsValidImage(
            byte[] header,
            string extension)
        {
            if (extension == ".jpg" || extension == ".jpeg")
            {
                return header[0] == 0xFF &&
                       header[1] == 0xD8 &&
                       header[2] == 0xFF;
            }

            if (extension == ".png")
            {
                byte[] png =
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                };

                return header.Take(8).SequenceEqual(png);
            }

            if (extension == ".webp")
            {
                return System.Text.Encoding.ASCII
                           .GetString(header, 0, 4) == "RIFF" &&
                       System.Text.Encoding.ASCII
                           .GetString(header, 8, 4) == "WEBP";
            }

            return false;
        }
    }
}
