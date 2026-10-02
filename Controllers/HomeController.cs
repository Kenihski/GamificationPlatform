using Microsoft.AspNetCore.Mvc;

using GamificationPlatform.DAL;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;

        public HomeController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
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
    }
}