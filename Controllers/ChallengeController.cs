using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public ChallengeController(ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        // Shows challenges in a table
        public async Task<IActionResult> Table()
        {
            List<Challenge> challenges =
                await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .ToListAsync();

            // Calculates max points from the questions
            foreach (var challenge in challenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var challengesViewModel =
                new ChallengesViewModel(challenges, "Table");

            return View(challengesViewModel);
        }

        // Shows challenges in a grid
        public async Task<IActionResult> Grid()
        {
            List<Challenge> challenges =
                await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .ToListAsync();

            // Calculates max points from the questions
            foreach (var challenge in challenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var challengesViewModel =
                new ChallengesViewModel(challenges, "Grid");

            return View(challengesViewModel);
        }

        // Shows information about a challenge and the user's history
        public async Task<IActionResult> Details(int id)
        {
            var challenge = await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.ChallengeId == id);

            if (challenge == null)
            {
                return NotFound();
            }

            // Calculates max points from the questions
            int maxPoints =
                challenge.Questions.Sum(q => q.Points);

            challenge.MaxPoints = maxPoints;

            var viewModel = new ChallengeDetailsHistoryViewModel
            {
                Challenge = challenge,
                MaxPoints = maxPoints
            };

            // Gets history only if the user is logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdString != null)
                {
                    int userId = int.Parse(userIdString);

                    // Gets all completed attempts
                    viewModel.Attempts =
                        await _challengeDbContext.ChallengeAttempts
                            .Where(a =>
                                a.UserChallenge.UserId == userId &&
                                a.UserChallenge.ChallengeId == id &&
                                a.Completed)
                            .OrderByDescending(a => a.CompletedAt)
                            .ToListAsync();

                    if (viewModel.Attempts.Any())
                    {
                        // Gets the latest completed attempt
                        viewModel.LatestAttemptId =
                            viewModel.Attempts
                                .First()
                                .ChallengeAttemptId;

                        // Gets the best completed attempt
                        viewModel.BestAttemptId =
                            viewModel.Attempts
                                .OrderByDescending(a => a.Score)
                                .ThenByDescending(a => a.CompletedAt)
                                .First()
                                .ChallengeAttemptId;

                        // Places the best attempt first
                        viewModel.Attempts =
                            viewModel.Attempts
                                .OrderByDescending(a =>
                                    a.ChallengeAttemptId ==
                                    viewModel.BestAttemptId)
                                .ThenByDescending(a => a.CompletedAt)
                                .ToList();
                    }
                }
            }

            return View(viewModel);
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

            var viewModel = new LeaderboardViewModel
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

        // Shows the answers from a completed attempt
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AttemptDetails(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            var attempt =
                await _challengeDbContext.ChallengeAttempts
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.Challenge)
                            .ThenInclude(c => c.Questions)
                    .Include(a => a.Answers)
                        .ThenInclude(answer => answer.Question)
                            .ThenInclude(question => question.Options)
                    .FirstOrDefaultAsync(a =>
                        a.ChallengeAttemptId == id &&
                        a.UserChallenge.UserId == userId &&
                        a.Completed);

            if (attempt == null)
            {
                return NotFound();
            }

            return View(attempt);
        }

        // Shows the challenge and its questions
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Take(int id)
        {
            var challenge =
                await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == id);

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

            var userChallenge =
                await _challengeDbContext.UserChallenges
                    .FirstOrDefaultAsync(uc =>
                        uc.UserId == userId &&
                        uc.ChallengeId == id);

            if (userChallenge == null)
            {
                userChallenge = new UserChallenge
                {
                    UserId = userId,
                    ChallengeId = id
                };

                _challengeDbContext.UserChallenges.Add(
                    userChallenge);

                await _challengeDbContext.SaveChangesAsync();
            }

            // Creates a new attempt
            var challengeAttempt = new ChallengeAttempt
            {
                UserChallengeId =
                    userChallenge.UserChallengeId,

                Score = 0,
                Completed = false,
                StartedAt = DateTime.Now
            };

            _challengeDbContext.ChallengeAttempts.Add(
                challengeAttempt);

            await _challengeDbContext.SaveChangesAsync();

            var viewModel = new TakeChallengeViewModel
            {
                Challenge = challenge,
                ChallengeAttemptId =
                    challengeAttempt.ChallengeAttemptId
            };

            return View(viewModel);
        }

        // Checks the answers, calculates the score
        // and saves the answers
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int challengeId,
            int challengeAttemptId,
            Dictionary<int, int>? answers)
        {
            answers ??= new Dictionary<int, int>();

            var challenge =
                await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == challengeId);

            if (challenge == null)
            {
                return NotFound();
            }

            var result = new ChallengeResultViewModel
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeTitle = challenge.Title,
                MaxScore =
                    challenge.Questions.Sum(q => q.Points)
            };

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            var challengeAttempt =
                await _challengeDbContext.ChallengeAttempts
                    .Include(a => a.UserChallenge)
                    .FirstOrDefaultAsync(a =>
                        a.ChallengeAttemptId ==
                            challengeAttemptId &&
                        a.UserChallenge.UserId ==
                            userId &&
                        a.UserChallenge.ChallengeId ==
                            challengeId &&
                        !a.Completed);

            if (challengeAttempt == null)
            {
                return NotFound();
            }

            foreach (var question in challenge.Questions)
            {
                int? selectedOptionId = null;
                bool isCorrect = false;

                if (answers.TryGetValue(
                    question.QuestionId,
                    out int optionId))
                {
                    selectedOptionId = optionId;

                    var selectedOption =
                        question.Options
                            .FirstOrDefault(o =>
                                o.QuestionOptionId ==
                                optionId);

                    if (selectedOption != null)
                    {
                        isCorrect =
                            selectedOption.IsCorrect;
                    }
                }

                if (isCorrect)
                {
                    result.Score += question.Points;
                }

                result.QuestionResults.Add(
                    new QuestionResultViewModel
                    {
                        Question = question,
                        SelectedOptionId =
                            selectedOptionId,
                        IsCorrect = isCorrect
                    });

                var attemptAnswer =
                    new AttemptAnswer
                    {
                        ChallengeAttemptId =
                            challengeAttempt
                                .ChallengeAttemptId,

                        QuestionId =
                            question.QuestionId,

                        SelectedOptionId =
                            selectedOptionId
                    };

                _challengeDbContext.AttemptAnswers.Add(
                    attemptAnswer);
            }

            // Saves the completed attempt
            challengeAttempt.Score = result.Score;
            challengeAttempt.Completed = true;
            challengeAttempt.CompletedAt = DateTime.Now;

            await _challengeDbContext.SaveChangesAsync();

            return View("Result", result);
        }

        // Shows the Create Challenge form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Creates a new challenge
        [HttpPost]
        public async Task<IActionResult> Create(
            Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Add(
                    challenge);

                await _challengeDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Table));
            }

            return View(challenge);
        }

        // Shows the Update Challenge form
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var challenge =
                await _challengeDbContext.Challenges
                    .FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // Updates an existing challenge
        [HttpPost]
        public async Task<IActionResult> Update(
            Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Update(
                    challenge);

                await _challengeDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Table));
            }

            return View(challenge);
        }

        // Shows the Delete Challenge confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var challenge =
                await _challengeDbContext.Challenges
                    .FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // Deletes a challenge
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var challenge =
                await _challengeDbContext.Challenges
                    .FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            _challengeDbContext.Challenges.Remove(
                challenge);

            await _challengeDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Table));
        }
    }
}