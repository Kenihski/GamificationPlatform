using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            ILogger<HomeController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            // Gets all published challenges.
            var challenges =
                await _challengeRepository
                    .GetPublishedChallengesAsync();


            // Keeps the challenge order consistent
            // for Challenge of the Day.
            challenges =
                challenges
                    .OrderBy(c => c.ChallengeId)
                    .ToList();


            var viewModel =
                new HomeViewModel();


            // If there are no published challenges,
            // the Home page can still be displayed.
            if (!challenges.Any())
            {
                return View(viewModel);
            }


            // Uses the day of the year to rotate
            // Challenge of the Day automatically.
            int challengeIndex =
                (DateTime.Today.DayOfYear - 1)
                % challenges.Count;


            var challengeOfTheDay =
                challenges[challengeIndex];


            // Calculates maximum points from
            // the questions in the challenge.
            int maxPoints =
                challengeOfTheDay.Questions
                    .Sum(q => q.Points);


            challengeOfTheDay.MaxPoints =
                maxPoints;


            viewModel.ChallengeOfTheDay =
                challengeOfTheDay;

            viewModel.MaxPoints =
                maxPoints;


            // Gets all completed attempts for
            // Challenge of the Day.
            var attempts =
                await _attemptRepository
                    .GetCompletedAttemptsForChallengeAsync(
                        challengeOfTheDay.ChallengeId);


            // Gets the best attempt from each user.
            // Highest score wins.
            // Shortest time is used when scores are equal.
            var bestAttempts =
                attempts
                    .GroupBy(a =>
                        a.UserChallenge.UserId)
                    .Select(group =>
                        group
                            .OrderByDescending(a =>
                                a.Score)
                            .ThenBy(a =>
                                a.CompletedAt -
                                a.StartedAt)
                            .First())
                    .OrderByDescending(a =>
                        a.Score)
                    .ThenBy(a =>
                        a.CompletedAt -
                        a.StartedAt)
                    .Take(3)
                    .ToList();


            int rank = 1;


            foreach (var attempt in bestAttempts)
            {
                viewModel.LeaderboardEntries.Add(
                    new LeaderboardEntryViewModel
                    {
                        Rank = rank,

                        Username =
                            attempt.UserChallenge
                                .User.Username,

                        Score =
                            attempt.Score,

                        StartedAt =
                            attempt.StartedAt,

                        CompletedAt =
                            attempt.CompletedAt
                    });


                rank++;
            }


            return View(viewModel);
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = 500;
            // Exception middleware and DAL already log the technical failure.
            return View(new ErrorViewModel
            {
                StatusCode = 500,
                Title = "Something went wrong",
                Message = "We could not complete your request. Please try again later.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ErrorStatus(int code)
        {
            if (code < 400 || code > 599) code = 404;
            Response.StatusCode = code;
            var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;
            _logger.LogWarning("Request {RequestId} to {RequestPath} returned HTTP {StatusCode} for user {UserId}.",
                HttpContext.TraceIdentifier, originalPath ?? Request.Path.Value, code,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");

            var (title, message) = code switch
            {
                400 => ("Invalid request", "Your request could not be processed. Check the form and try again."),
                401 => ("Login required", "Please log in to continue."),
                403 => ("Access denied", "You do not have permission to perform this action."),
                404 => ("Page not found", "The requested page or resource could not be found."),
                _ => ("Request failed", "We could not complete your request. Please try again later.")
            };
            return View("Error", new ErrorViewModel
            {
                StatusCode = code, Title = title, Message = message,
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }
}
