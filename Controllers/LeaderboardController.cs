using Microsoft.AspNetCore.Mvc;

using GamificationPlatform.DAL;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;

        public LeaderboardController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
        }


        // Shows the leaderboard for a challenge.
        [HttpGet]
        public async Task<IActionResult> Leaderboard(int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeWithQuestionsAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            int maxPoints =
                challenge.Questions.Sum(q => q.Points);


            // Gets all completed attempts
            // for this challenge.
            var attempts =
                await _attemptRepository
                    .GetCompletedAttemptsForChallengeAsync(id);


            // Gets the best attempt from each user.
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
                    .ToList();


            var viewModel =
                new LeaderboardViewModel
                {
                    Challenge = challenge,
                    MaxPoints = maxPoints
                };


            int rank = 1;


            foreach (var attempt in bestAttempts)
            {
                viewModel.Entries.Add(
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