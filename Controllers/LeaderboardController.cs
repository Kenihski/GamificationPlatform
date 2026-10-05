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


            // The challenge creator is excluded
            // from their own leaderboard.
            attempts =
                attempts
                    .Where(a =>
                        a.UserChallenge.UserId !=
                        challenge.CreatedByUserId)
                    .ToList();


            // Only the first completed attempt
            // from each user can count.
            var firstAttempts =
                attempts
                    .GroupBy(a =>
                        a.UserChallenge.UserId)
                    .Select(group =>
                        group
                            .OrderBy(a =>
                                a.CompletedAt)
                            .ThenBy(a =>
                                a.ChallengeAttemptId)
                            .First())
                    .ToList();


            // Users with zero points on their first attempt
            // are not shown on the leaderboard.
            firstAttempts =
                firstAttempts
                    .Where(a =>
                        a.Score > 0)
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


            int rank = 0;
            int? previousScore = null;
            TimeSpan? previousTime = null;


            foreach (var attempt in firstAttempts)
            {
                TimeSpan timeUsed =
                    attempt.CompletedAt!.Value -
                    attempt.StartedAt;


                // Dense ranking:
                // equal score and time share the same rank.
                if (previousScore == null ||
                    attempt.Score != previousScore ||
                    timeUsed != previousTime)
                {
                    rank++;
                }


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


                previousScore =
                    attempt.Score;

                previousTime =
                    timeUsed;
            }


            return View(viewModel);
        }


        // Shows the total leaderboard
        // based only on Core Challenges.
        [HttpGet]
        public async Task<IActionResult> TotalLeaderboard()
        {
            var attempts =
                await _attemptRepository
                    .GetAllCompletedAttemptsAsync();


            // Only Core Challenges count.
            attempts =
                attempts
                    .Where(a =>
                        a.UserChallenge.Challenge.IsCore)
                    .ToList();


            // The challenge creator is excluded
            // from their own challenge.
            attempts =
                attempts
                    .Where(a =>
                        a.UserChallenge.UserId !=
                        a.UserChallenge.Challenge.CreatedByUserId)
                    .ToList();


            // Only the first completed attempt
            // from each user on each Core Challenge can count.
            var firstAttempts =
                attempts
                    .GroupBy(a => new
                    {
                        a.UserChallenge.UserId,
                        a.UserChallenge.ChallengeId
                    })
                    .Select(group =>
                        group
                            .OrderBy(a =>
                                a.CompletedAt)
                            .ThenBy(a =>
                                a.ChallengeAttemptId)
                            .First())
                    .ToList();


            // A zero score on the first attempt
            // does not contribute to the leaderboard.
            firstAttempts =
                firstAttempts
                    .Where(a =>
                        a.Score > 0)
                    .ToList();


            var entries =
                firstAttempts
                    .GroupBy(a =>
                        a.UserChallenge.UserId)
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
                                    .User.Username,

                            TotalPoints =
                                Math.Round(totalPoints, 1),

                            CoreChallengesCompleted =
                                group.Count(),

                            TotalTime =
                                totalTime
                        };
                    })
                    .OrderByDescending(entry =>
                        entry.TotalPoints)
                    .ThenBy(entry =>
                        entry.CoreChallengesCompleted)
                    .ThenBy(entry =>
                        entry.TotalTime)
                    .ToList();


            // Dense ranking:
            // completely equal results share the same rank.
            int rank = 0;
            double? previousPoints = null;
            int? previousChallenges = null;
            TimeSpan? previousTime = null;


            foreach (var entry in entries)
            {
                if (previousPoints == null ||
                    entry.TotalPoints != previousPoints ||
                    entry.CoreChallengesCompleted != previousChallenges ||
                    entry.TotalTime != previousTime)
                {
                    rank++;
                }


                entry.Rank =
                    rank;


                previousPoints =
                    entry.TotalPoints;

                previousChallenges =
                    entry.CoreChallengesCompleted;

                previousTime =
                    entry.TotalTime;
            }


            var viewModel =
                new TotalLeaderboardViewModel
                {
                    Entries = entries
                };


            return View(viewModel);
        }
    }
}