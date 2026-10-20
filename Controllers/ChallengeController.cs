
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using GamificationPlatform.Services;

namespace GamificationPlatform.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly IImageService _imageService;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<ChallengeController> _logger;

        public ChallengeController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            IImageService imageService,
            IAchievementService achievementService,
            ILogger<ChallengeController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _imageService = imageService;
            _achievementService = achievementService;
            _logger = logger;
        }

        // Shows published challenges in a table.
        public async Task<IActionResult> Table(
            string filter = "All",
            string? searchTerm = null)
        {
            var viewModel = await BuildChallengesViewModelAsync(
                filter, "Table", searchTerm);

            return View(viewModel);
        }

        // Shows published challenges in a grid.
        public async Task<IActionResult> Grid(
            string filter = "All",
            string? searchTerm = null)
        {
            var viewModel = await BuildChallengesViewModelAsync(
                filter, "Grid", searchTerm);

            return View(viewModel);
        }

        // Prepares challenge data shared by the Table and Grid views.
        private async Task<ChallengesViewModel> BuildChallengesViewModelAsync(
            string filter,
            string viewMode,
            string? searchTerm)
        {
            if (filter != "Core" && filter != "Community")
            {
                filter = "All";
            }

            List<Challenge> challenges =
                await _challengeRepository.SearchPublishedChallengesAsync(
                    filter, searchTerm);

            foreach (var challenge in challenges)
            {
                challenge.MaxPoints = challenge.Questions.Sum(q => q.Points);
            }

            var completedChallengeIds = new List<int>();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdString != null)
                {
                    int userId = int.Parse(userIdString);

                    completedChallengeIds = await _attemptRepository
                        .GetCompletedChallengeIdsAsync(userId);
                }
            }

            return new ChallengesViewModel(
                challenges,
                viewMode,
                filter,
                completedChallengeIds,
                searchTerm);
        }

        // Shows challenges created by the logged-in user.
        [Authorize]
        public async Task<IActionResult> MyChallenges()
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            var challenges =
                await _challengeRepository.GetChallengesByUserIdAsync(userId);

            var viewModel = new MyChallengesViewModel();

            foreach (var challenge in challenges)
            {
                viewModel.Challenges.Add(new MyChallengeViewModel
                {
                    ChallengeId = challenge.ChallengeId,
                    Title = challenge.Title,
                    QuestionCount = challenge.Questions.Count,
                    MaxPoints = challenge.Questions.Sum(q => q.Points),
                    IsPublished = challenge.IsPublished,
                    IsCore = challenge.IsCore
                });
            }

            return View(viewModel);
        }

        // Publishes a challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id, string? returnUrl)
        {
            var challenge =
                await _challengeRepository.GetChallengeWithQuestionsAsync(id);

            if (challenge == null)
            {
                _logger.LogWarning(
                    "Publish failed because challenge {ChallengeId} was not found.",
                    id);

                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to publish challenge {ChallengeId} without permission.",
                    userId,
                    id);

                return Forbid();
            }

            if (!challenge.Questions.Any())
            {
                _logger.LogWarning(
                    "Challenge {ChallengeId} could not be published because it has no questions.",
                    id);

                TempData["ErrorMessage"] =
                    "The challenge must have at least one question before it can be published.";

                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction(nameof(MyChallenges));
            }

            challenge.IsPublished = true;

            await _challengeRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} was published by user {UserId}.",
                id,
                userId);

            TempData["SuccessMessage"] =
                "Challenge published successfully.";

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(MyChallenges));
        }

        // Unpublishes a challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(
            int id,
            string? returnUrl)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied challenge management access.",
                    userId);

                return Forbid();
            }

            challenge.IsPublished = false;

            await _challengeRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} was unpublished by user {UserId}.",
                id,
                userId);

            TempData["SuccessMessage"] =
                "Challenge unpublished successfully.";

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(MyChallenges));
        }

        // Marks a challenge as Core.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeCore(
            int id,
            string? returnUrl)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            challenge.IsCore = true;

            await _challengeRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} was marked as Core.",
                id);

            TempData["SuccessMessage"] = "Challenge marked as Core.";

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // Marks a challenge as Community.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeCommunity(
            int id,
            string? returnUrl)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            challenge.IsCore = false;

            await _challengeRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Challenge {ChallengeId} was marked as Community.",
                id);

            TempData["SuccessMessage"] = "Challenge marked as Community.";

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // Shows challenge details and attempt history.
        public async Task<IActionResult> Details(
            int id,
            bool showQuestions = false)
        {
            var challenge =
                await _challengeRepository.GetChallengeWithQuestionsAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            int maxPoints = challenge.Questions.Sum(q => q.Points);
            challenge.MaxPoints = maxPoints;

            var viewModel = new ChallengeDetailsHistoryViewModel
            {
                Challenge = challenge,
                MaxPoints = maxPoints,
                ShowQuestions = showQuestions
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdString != null)
                {
                    int userId = int.Parse(userIdString);

                    viewModel.CanManage =
                        User.IsInRole("Admin") ||
                        challenge.CreatedByUserId == userId;

                    var attempts = await _attemptRepository
                        .GetCompletedAttemptsAsync(userId, id);

                    if (attempts.Any())
                    {
                        var chronologicalAttempts = attempts
                            .OrderBy(a => a.CompletedAt)
                            .ToList();

                        var latestAttempt = attempts.First();

                        var bestAttempt = attempts
                            .OrderByDescending(a => a.Score)
                            .ThenByDescending(a => a.CompletedAt)
                            .First();

                        attempts = attempts
                            .OrderByDescending(a =>
                                a.ChallengeAttemptId ==
                                bestAttempt.ChallengeAttemptId)
                            .ThenByDescending(a => a.CompletedAt)
                            .ToList();

                        foreach (var attempt in attempts)
                        {
                            int attemptNumber =
                                chronologicalAttempts.FindIndex(a =>
                                    a.ChallengeAttemptId ==
                                    attempt.ChallengeAttemptId) + 1;

                            string? timeUsed = null;

                            if (attempt.CompletedAt.HasValue)
                            {
                                var duration =
                                    attempt.CompletedAt.Value -
                                    attempt.StartedAt;

                                if (duration.TotalMinutes >= 1)
                                {
                                    timeUsed =
                                        $"{Math.Round(duration.TotalMinutes)} min";
                                }
                                else
                                {
                                    timeUsed =
                                        $"{Math.Round(duration.TotalSeconds)} sec";
                                }
                            }

                            viewModel.Attempts.Add(
                                new ChallengeAttemptHistoryViewModel
                                {
                                    ChallengeAttemptId =
                                        attempt.ChallengeAttemptId,
                                    AttemptNumber = attemptNumber,
                                    Score = attempt.Score,
                                    TimeUsed = timeUsed,
                                    CompletedAt = attempt.CompletedAt?
                                        .ToString("dd.MM.yyyy HH:mm"),
                                    IsBest =
                                        attempt.ChallengeAttemptId ==
                                        bestAttempt.ChallengeAttemptId,
                                    IsLatest =
                                        attempt.ChallengeAttemptId ==
                                        latestAttempt.ChallengeAttemptId
                                });
                        }
                    }
                }
            }

            return View(viewModel);
        }

        // Loads images for the Challenge Create and Update forms.
        private async Task LoadImagesAsync(int userId)
        {
            ViewBag.MyImages =
                await _imageService.GetUserImagesAsync(userId);

            ViewData["DefaultImages"] =
                await _imageService.GetDefaultImagesAsync();
        }

        // Shows the Create Challenge form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            await LoadImagesAsync(userId);

            return View();
        }

        // Creates a new challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Challenge challenge,
            IFormFile? imageUpload)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            // The logged-in user becomes the owner.
            challenge.CreatedByUserId = userId;

            // MaxPoints is calculated from questions later.
            challenge.MaxPoints = 0;

            // New challenges start as drafts.
            challenge.IsPublished = false;

            // New challenges are Community by default.
            challenge.IsCore = false;

            // Navigation property is not received from the form.
            ModelState.Remove("CreatedByUser");

            // ImageUrl may be empty when uploading a new image
            // or when the user chooses No image.
            ModelState.Remove("ImageUrl");

            // The user cannot upload and select an existing
            // image at the same time.
            if (imageUpload != null &&
                !string.IsNullOrWhiteSpace(challenge.ImageUrl))
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "Choose either an existing image or an upload.");
            }

            // Verify that the user is allowed to use this image.
            if (!await _imageService.IsAllowedImageAsync(
                challenge.ImageUrl, userId))
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "This image is not available to you.");
            }

            // Save the uploaded image if validation succeeds.
            if (ModelState.IsValid && imageUpload != null)
            {
                var result =
                    await _imageService.SaveImageAsync(imageUpload, userId);

                if (result.IsSuccess)
                {
                    challenge.ImageUrl = result.ImageUrl!;
                }
                else
                {
                    ModelState.AddModelError(
                        "imageUpload",
                        result.ErrorMessage!);
                }
            }

            if (ModelState.IsValid)
            {
                challenge = new Challenge
                {
                    Title = challenge.Title,
                    Description = challenge.Description,
                    ImageUrl = challenge.ImageUrl ?? string.Empty,
                    TimeLimitMinutes = challenge.TimeLimitMinutes,
                    ShowAnswersAfterCompletion = challenge.ShowAnswersAfterCompletion,
                    CreatedByUserId = userId,
                    IsPublished = false,
                    IsCore = false
                };

                await _challengeRepository
                    .CreateChallengeAsync(challenge);

                // Check whether creating the challenge unlocked
                // any creator achievements.
                await _achievementService
                    .CheckAchievementsAsync(userId);

                _logger.LogInformation(
                    "Challenge {ChallengeId} was created by user {UserId}.",
                    challenge.ChallengeId,
                    userId);

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = challenge.ChallengeId
                    });
            }

            _logger.LogWarning(
                "Challenge creation rejected because validation failed for user {UserId}.",
                userId);

            await LoadImagesAsync(userId);

            return View(challenge);
        }

        // Shows the Update Challenge form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied challenge management access.",
                    userId);

                return Forbid();
            }

            await LoadImagesAsync(userId);

            return View(challenge);
        }

        // Updates an existing challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Challenge challenge,
            IFormFile? imageUpload)
        {
            var existingChallenge =
                await _challengeRepository.GetChallengeByIdAsync(
                    challenge.ChallengeId);

            if (existingChallenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin &&
                existingChallenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied challenge management access.",
                    userId);

                return Forbid();
            }

            // Navigation property is not received from the form.
            ModelState.Remove("CreatedByUser");

            // ImageUrl may be empty when uploading a new image
            // or when the user chooses No image.
            ModelState.Remove("ImageUrl");

            if (imageUpload != null &&
                !string.IsNullOrWhiteSpace(challenge.ImageUrl))
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "Choose either an existing image or an upload.");
            }

            // The current image may belong to another user.
            // Admins can keep that image, but cannot select
            // a different image belonging to another user.
            bool keepingExistingImage =
                challenge.ImageUrl == existingChallenge.ImageUrl;

            if (!keepingExistingImage &&
                !await _imageService.IsAllowedImageAsync(
                    challenge.ImageUrl, userId))
            {
                ModelState.AddModelError(
                    "ImageUrl",
                    "This image is not available to you.");
            }

            if (ModelState.IsValid && imageUpload != null)
            {
                var result =
                    await _imageService.SaveImageAsync(imageUpload, userId);

                if (result.IsSuccess)
                {
                    challenge.ImageUrl = result.ImageUrl!;
                }
                else
                {
                    ModelState.AddModelError(
                        "imageUpload",
                        result.ErrorMessage!);
                }
            }

            if (ModelState.IsValid)
            {
                // Only update fields the user is allowed to change.
                existingChallenge.Title = challenge.Title;

                existingChallenge.Description = challenge.Description;

                existingChallenge.ImageUrl =
                    challenge.ImageUrl ?? string.Empty;

                existingChallenge.TimeLimitMinutes =
                    challenge.TimeLimitMinutes;

                existingChallenge.ShowAnswersAfterCompletion =
                    challenge.ShowAnswersAfterCompletion;

                // CreatedByUserId is not changed.
                // The original owner stays the owner.
                // IsCore is only changed by admin actions.

                await _challengeRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Challenge {ChallengeId} was updated by user {UserId}.",
                    existingChallenge.ChallengeId,
                    userId);

                return RedirectToAction(nameof(Table));
            }

            _logger.LogWarning(
                "Challenge update rejected because validation failed for user {UserId} on challenge {ChallengeId}.",
                userId,
                existingChallenge.ChallengeId);

            await LoadImagesAsync(userId);

            return View(challenge);
        }

        // Shows the Delete Challenge confirmation page.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied challenge management access.",
                    userId);

                return Forbid();
            }

            return View(challenge);
        }

        // Deletes a challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied challenge management access.",
                    userId);

                return Forbid();
            }

            await _challengeRepository.DeleteChallengeAsync(challenge);

            _logger.LogInformation(
                "Challenge {ChallengeId} was deleted by user {UserId}.",
                id,
                userId);

            return RedirectToAction(nameof(MyChallenges));
        }
    }
}
