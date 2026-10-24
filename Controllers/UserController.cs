
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // Shows all users - admin only.
        public async Task<IActionResult> Table()
        {
            List<User> users =
                await _userRepository.GetAllUsersAsync();

            return View(users);
        }

        // Shows details and history for one user - admin only.
        public async Task<IActionResult> Details(int id)
        {
            var user =
                await _userRepository.GetUserWithDetailsAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Calculates max points for challenges
            // created by this user.
            foreach (var challenge in user.CreatedChallenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                CreatedChallenges = user.CreatedChallenges
            };

            // Prepares completed challenge history
            // for the view.
            foreach (var userChallenge in user.UserChallenges)
            {
                var attempts = userChallenge.Attempts
                    .Where(a => a.Completed)
                    .OrderByDescending(a => a.CompletedAt)
                    .ToList();

                if (!attempts.Any())
                {
                    continue;
                }

                var chronologicalAttempts = attempts
                    .OrderBy(a => a.CompletedAt)
                    .ToList();

                var latestAttempt = attempts.First();

                var bestAttempt = attempts
                    .OrderByDescending(a => a.Score)
                    .ThenBy(a =>
                        (a.CompletedAt ?? a.StartedAt) - a.StartedAt)
                    .First();

                int maxPoints = userChallenge.Challenge
                    .Questions.Sum(q => q.Points);

                var challengeHistory =
                    new UserChallengeHistoryViewModel
                    {
                        ChallengeTitle =
                            userChallenge.Challenge.Title,

                        MaxPoints = maxPoints
                    };

                foreach (var attempt in attempts)
                {
                    int attemptNumber = chronologicalAttempts
                        .FindIndex(a =>
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

                    challengeHistory.Attempts.Add(
                        new UserAttemptHistoryViewModel
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

                viewModel.ChallengeHistory.Add(challengeHistory);
            }

            return View(viewModel);
        }
    }
}