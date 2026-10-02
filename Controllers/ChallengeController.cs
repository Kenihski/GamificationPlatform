using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class ChallengeController : Controller
    {
        // Repositories handle database operations for challenges and attempts.
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;

        // Logger records important challenge operations and problems.
        private readonly ILogger<ChallengeController> _logger;

        public ChallengeController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            ILogger<ChallengeController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _logger = logger;
        }


        // Shows published challenges in a table.
        public async Task<IActionResult> Table()
        {
            List<Challenge> challenges =
                await _challengeRepository
                    .GetPublishedChallengesAsync();

            // Calculates max points from the questions.
            foreach (var challenge in challenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var challengesViewModel =
                new ChallengesViewModel(
                    challenges,
                    "Table");

            return View(challengesViewModel);
        }


        // Shows published challenges in a grid.
        public async Task<IActionResult> Grid()
        {
            List<Challenge> challenges =
                await _challengeRepository
                    .GetPublishedChallengesAsync();

            // Calculates max points from the questions.
            foreach (var challenge in challenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var challengesViewModel =
                new ChallengesViewModel(
                    challenges,
                    "Grid");

            return View(challengesViewModel);
        }


        // Shows challenges created by the logged-in user.
        [Authorize]
        public async Task<IActionResult> MyChallenges()
        {
            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var challenges =
                await _challengeRepository
                    .GetChallengesByUserIdAsync(userId);

            // Prepares the challenge information
            // needed by the view.
            var viewModel =
                new MyChallengesViewModel();

            foreach (var challenge in challenges)
            {
                viewModel.Challenges.Add(
                    new MyChallengeViewModel
                    {
                        ChallengeId =
                            challenge.ChallengeId,

                        Title =
                            challenge.Title,

                        QuestionCount =
                            challenge.Questions.Count,

                        MaxPoints =
                            challenge.Questions
                                .Sum(q => q.Points),

                        IsPublished =
                            challenge.IsPublished
                    });
            }

            return View(viewModel);
        }


        // Publishes a challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(
            int id,
            string? returnUrl)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeWithQuestionsAsync(id);

            if (challenge == null)
            {
                _logger.LogWarning(
                    "Publish failed because challenge {ChallengeId} was not found.",
                    id);

                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to publish challenge {ChallengeId} without permission.",
                    userId,
                    id);

                return Forbid();
            }

            // A challenge must have at least one
            // question before publishing.
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

                return RedirectToAction(
                    nameof(MyChallenges));
            }

            challenge.IsPublished = true;

            await _challengeRepository
                .SaveChangesAsync();

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

            return RedirectToAction(
                nameof(MyChallenges));
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
                await _challengeRepository
                    .GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            // Normal users can only unpublish
            // challenges they created.
            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            challenge.IsPublished = false;

            await _challengeRepository
                .SaveChangesAsync();

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

            return RedirectToAction(
                nameof(MyChallenges));
        }


        // Shows information about a challenge
        // and the user's attempt history.
        public async Task<IActionResult> Details(int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeWithQuestionsAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            // Calculates max points from the questions.
            int maxPoints =
                challenge.Questions.Sum(q => q.Points);

            challenge.MaxPoints = maxPoints;

            var viewModel =
                new ChallengeDetailsHistoryViewModel
                {
                    Challenge = challenge,
                    MaxPoints = maxPoints
                };

            // Gets history and management permission
            // only if the user is logged in.
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (userIdString != null)
                {
                    int userId =
                        int.Parse(userIdString);

                    // The challenge owner and admins
                    // can manage the challenge.
                    viewModel.CanManage =
                        User.IsInRole("Admin") ||
                        challenge.CreatedByUserId == userId;

                    // Gets the user's completed attempts
                    // for this challenge.
                    viewModel.Attempts =
                        await _attemptRepository
                            .GetCompletedAttemptsAsync(
                                userId,
                                id);

                    if (viewModel.Attempts.Any())
                    {
                        // Gets the latest completed attempt.
                        viewModel.LatestAttemptId =
                            viewModel.Attempts
                                .First()
                                .ChallengeAttemptId;

                        // Gets the best completed attempt.
                        viewModel.BestAttemptId =
                            viewModel.Attempts
                                .OrderByDescending(a =>
                                    a.Score)
                                .ThenByDescending(a =>
                                    a.CompletedAt)
                                .First()
                                .ChallengeAttemptId;

                        // Places the best attempt first.
                        viewModel.Attempts =
                            viewModel.Attempts
                                .OrderByDescending(a =>
                                    a.ChallengeAttemptId ==
                                    viewModel.BestAttemptId)
                                .ThenByDescending(a =>
                                    a.CompletedAt)
                                .ToList();
                    }
                }
            }

            return View(viewModel);
        }


        // Shows the Create Challenge form.
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // Creates a new challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Challenge challenge)
        {
            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            // The logged-in user becomes the owner.
            challenge.CreatedByUserId =
                userId;

            // MaxPoints is calculated from questions later.
            challenge.MaxPoints = 0;

            // New challenges start as drafts.
            challenge.IsPublished = false;

            // Navigation property is not received from the form.
            ModelState.Remove("CreatedByUser");

            if (ModelState.IsValid)
            {
                await _challengeRepository
                    .CreateChallengeAsync(challenge);

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

            return View(challenge);
        }


        // Shows the Update Challenge form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            // Normal users can only update
            // challenges they created.
            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            return View(challenge);
        }


        // Updates an existing challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Challenge challenge)
        {
            var existingChallenge =
                await _challengeRepository
                    .GetChallengeByIdAsync(
                        challenge.ChallengeId);

            if (existingChallenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            // Normal users can only update
            // challenges they created.
            if (!isAdmin &&
                existingChallenge.CreatedByUserId !=
                    userId)
            {
                return Forbid();
            }

            // Navigation property is not
            // received from the form.
            ModelState.Remove("CreatedByUser");

            if (ModelState.IsValid)
            {
                // Only updates fields the user
                // is allowed to change.
                existingChallenge.Title =
                    challenge.Title;

                existingChallenge.Description =
                    challenge.Description;

                existingChallenge.ImageUrl =
                    challenge.ImageUrl;

                existingChallenge.TimeLimitMinutes =
                    challenge.TimeLimitMinutes;

                // CreatedByUserId is not changed.
                // The original owner stays the owner.

                await _challengeRepository
                    .SaveChangesAsync();

                _logger.LogInformation(
                    "Challenge {ChallengeId} was updated by user {UserId}.",
                    existingChallenge.ChallengeId,
                    userId);

                return RedirectToAction(
                    nameof(Table));
            }

            return View(challenge);
        }


        // Shows the Delete Challenge confirmation page.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            // Normal users can only delete
            // challenges they created.
            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            return View(challenge);
        }


        // Deletes a challenge.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            // Normal users can only delete
            // challenges they created.
            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            await _challengeRepository
                .DeleteChallengeAsync(challenge);

            _logger.LogInformation(
                "Challenge {ChallengeId} was deleted by user {UserId}.",
                id,
                userId);

            return RedirectToAction(
                nameof(Table));
        }
    }
}