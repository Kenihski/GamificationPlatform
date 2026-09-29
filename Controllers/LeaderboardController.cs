using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public LeaderboardController(
            ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }


        // Shows the leaderboard for a challenge
        [HttpGet]
        public async Task<IActionResult> Leaderboard(int id)
        {
            var challenge =
                await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == id);


            if (challenge == null)
            {
                return NotFound();
            }


            int maxPoints =
                challenge.Questions.Sum(q => q.Points);


            // Gets all completed attempts for this challenge
            var attempts =
                await _challengeDbContext.ChallengeAttempts
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.User)
                    .Where(a =>
                        a.UserChallenge.ChallengeId == id &&
                        a.Completed)
                    .ToListAsync();


            // Gets the best attempt from each user
            var bestAttempts =
                attempts
                    .GroupBy(a => a.UserChallenge.UserId)
                    .Select(group =>
                        group
                            .OrderByDescending(a => a.Score)
                            .ThenBy(a =>
                                a.CompletedAt - a.StartedAt)
                            .First())
                    .OrderByDescending(a => a.Score)
                    .ThenBy(a =>
                        a.CompletedAt - a.StartedAt)
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
                            attempt.UserChallenge.User.Username,

                        Score = attempt.Score,

                        StartedAt = attempt.StartedAt,

                        CompletedAt = attempt.CompletedAt
                    });


                rank++;
            }


            return View(viewModel);
        }
    }
}