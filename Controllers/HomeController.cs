
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IAchievementRepository _achievementRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IAchievementRepository achievementRepository,
            ILogger<HomeController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _achievementRepository = achievementRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // Gets all published challenges.
            var challenges =
                await _challengeRepository
                    .GetPublishedChallengesAsync();

            // Selects up to five random featured challenges.
            var featuredChallenges = challenges
                .OrderBy(_ => Random.Shared.Next())
                .Take(5)
                .ToList();

            // Admins may test quizzes but do not participate in leaderboards.
            var adminUsers =
                await _userManager.GetUsersInRoleAsync("Admin");

            var adminIds = adminUsers
                .Select(u => u.Id)
                .ToHashSet();

            // Gets completed attempts for popularity and leaderboards.
            var attempts =
                await _attemptRepository
                    .GetAllCompletedAttemptsAsync();

            // Counts unique participants for each challenge.
            // Admins and challenge creators are excluded.
            var participantCounts = attempts
                .Where(a =>
                    !adminIds.Contains(a.UserChallenge.UserId) &&
                    a.UserChallenge.UserId !=
                        a.UserChallenge.Challenge.CreatedByUserId)
                .GroupBy(a => a.UserChallenge.ChallengeId)
                .Select(group => new
                {
                    ChallengeId = group.Key,
                    Participants = group
                        .Select(a => a.UserChallenge.UserId)
                        .Distinct()
                        .Count()
                })
                .ToList();

            // The five most popular published challenges
            // need at least one unique participant.
            var popularChallengeIds = participantCounts
                .Where(entry =>
                    entry.Participants >= 1 &&
                    challenges.Any(c =>
                        c.ChallengeId == entry.ChallengeId))
                .OrderByDescending(entry => entry.Participants)
                .ThenBy(entry => entry.ChallengeId)
                .Take(5)
                .Select(entry => entry.ChallengeId)
                .ToHashSet();

            // Builds the featured challenge cards.
            foreach (var challenge in featuredChallenges)
            {
                var creator = await _userRepository
                    .GetUserByIdAsync(challenge.CreatedByUserId);

                viewModel.FeaturedChallenges.Add(
                    new FeaturedChallengeViewModel
                    {
                        Challenge = challenge,

                        MaxPoints = challenge.Questions
                            .Sum(q => q.Points),

                        CreatorUsername =
                            creator?.UserName ?? "Unknown",

                        IsPopular = popularChallengeIds
                            .Contains(challenge.ChallengeId)
                    });
            }

            // Core Leaderboard

            // Only Core Challenges count.
            // Challenge creators are excluded.
            var coreAttempts = attempts
                .Where(a =>
                    a.UserChallenge.Challenge.IsCore &&
                    !adminIds.Contains(a.UserChallenge.UserId) &&
                    a.UserChallenge.UserId !=
                        a.UserChallenge.Challenge.CreatedByUserId)
                .ToList();

            // Only the first completed attempt
            // per user and Core Challenge counts.
            var firstAttempts =
                coreAttempts
                    .GroupBy(a => new
                    {
                        a.UserChallenge.UserId,
                        a.UserChallenge.ChallengeId
                    })
                    .Select(group =>
                        group
                            .OrderBy(a => a.CompletedAt)
                            .ThenBy(a => a.ChallengeAttemptId)
                            .First())
                    .Where(a => a.Score > 0)
                    .ToList();

            // Calculates normalized total points.
            var coreEntries =
                firstAttempts
                    .GroupBy(a => a.UserChallenge.UserId)
                    .Select(group =>
                    {
                        double totalPoints =
                            group.Sum(attempt =>
                            {
                                int maxPoints =
                                    attempt.UserChallenge
                                        .Challenge
                                        .Questions
                                        .Sum(q => q.Points);

                                if (maxPoints <= 0)
                                {
                                    return 0;
                                }

                                return
                                    (double)attempt.Score /
                                    maxPoints * 100;
                            });

                        TimeSpan totalTime =
                            TimeSpan.FromTicks(
                                group.Sum(attempt =>
                                    (attempt.CompletedAt!.Value -
                                     attempt.StartedAt).Ticks));

                        return new TotalLeaderboardEntryViewModel
                        {
                            Username =
                                group.First()
                                    .UserChallenge
                                    .User.UserName ?? "",

                            TotalPoints =
                                Math.Round(totalPoints, 1),

                            CoreChallengesCompleted =
                                group.Count(),

                            TotalTime =
                                totalTime
                        };
                    })
                    .OrderByDescending(entry => entry.TotalPoints)
                    .ThenBy(entry => entry.CoreChallengesCompleted)
                    .ThenBy(entry => entry.TotalTime)
                    .ToList();

            // Dense ranking: equal results share a rank.
            int coreRank = 0;
            double? previousCorePoints = null;
            int? previousChallenges = null;
            TimeSpan? previousCoreTime = null;

            foreach (var entry in coreEntries)
            {
                if (previousCorePoints == null ||
                    entry.TotalPoints != previousCorePoints ||
                    entry.CoreChallengesCompleted != previousChallenges ||
                    entry.TotalTime != previousCoreTime)
                {
                    coreRank++;
                }

                entry.Rank = coreRank;

                previousCorePoints = entry.TotalPoints;
                previousChallenges = entry.CoreChallengesCompleted;
                previousCoreTime = entry.TotalTime;
            }

            viewModel.CoreLeaderboardEntries =
                coreEntries.Take(3).ToList();

            // Hall of Fame

            var users =
                (await _userRepository.GetAllUsersAsync())
                    .Where(u => !adminIds.Contains(u.Id))
                    .ToList();

            var hallOfFameEntries =
                new List<HallOfFameUserViewModel>();

            foreach (var user in users)
            {
                var userAchievements =
                    await _achievementRepository
                        .GetUserAchievementsAsync(user.Id);

                // Only active, non-goal achievements
                // contribute Achievement Points.
                int achievementPoints =
                    userAchievements
                        .Where(ua =>
                            ua.Achievement.IsActive &&
                            !ua.Achievement.IsGoal)
                        .Sum(ua => ua.Achievement.Points);

                hallOfFameEntries.Add(
                    new HallOfFameUserViewModel
                    {
                        UserId = user.Id,
                        Username = user.UserName ?? "",
                        AchievementPoints = achievementPoints
                    });
            }

            hallOfFameEntries =
                hallOfFameEntries
                    .OrderByDescending(u => u.AchievementPoints)
                    .ThenBy(u => u.Username)
                    .ToList();

            // Users with equal AP share a rank.
            int hallRank = 0;
            int? previousAchievementPoints = null;

            foreach (var entry in hallOfFameEntries)
            {
                if (previousAchievementPoints == null ||
                    entry.AchievementPoints !=
                    previousAchievementPoints)
                {
                    hallRank++;
                }

                entry.Rank = hallRank;

                previousAchievementPoints =
                    entry.AchievementPoints;
            }

            viewModel.HallOfFameEntries =
                hallOfFameEntries.Take(3).ToList();

            return View(viewModel);
        }

        // Displays information about the platform and project.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = 500;

            // Exception middleware and DAL
            // already log the technical failure.
            return View(new ErrorViewModel
            {
                StatusCode = 500,
                Title = "Something went wrong",
                Message = "We could not complete your request. Please try again later.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult ErrorStatus(int code)
        {
            if (code < 400 || code > 599)
            {
                code = 404;
            }

            Response.StatusCode = code;

            var originalPath =
                HttpContext.Features
                    .Get<IStatusCodeReExecuteFeature>()
                    ?.OriginalPath;

            _logger.LogWarning(
                "Request {RequestId} to {RequestPath} returned HTTP {StatusCode} for user {UserId}.",
                HttpContext.TraceIdentifier,
                originalPath ?? Request.Path.Value,
                code,
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? "anonymous");

            var (title, message) = code switch
            {
                400 => (
                    "Invalid request",
                    "Your request could not be processed. Check the form and try again."),

                401 => (
                    "Login required",
                    "Please log in to continue."),

                403 => (
                    "Access denied",
                    "You do not have permission to perform this action."),

                404 => (
                    "Page not found",
                    "The requested page or resource could not be found."),

                _ => (
                    "Request failed",
                    "We could not complete your request. Please try again later.")
            };

            return View("Error", new ErrorViewModel
            {
                StatusCode = code,
                Title = title,
                Message = message,
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }
}
