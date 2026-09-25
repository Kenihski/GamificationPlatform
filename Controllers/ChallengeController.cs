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
                        // If several attempts have the same score,
                        // the newest one is considered best
                        viewModel.BestAttemptId =
                            viewModel.Attempts
                                .OrderByDescending(a => a.Score)
                                .ThenByDescending(a => a.CompletedAt)
                                .First()
                                .ChallengeAttemptId;

                        // Places the best attempt first,
                        // then the remaining attempts newest first
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

        // Shows the answers from a completed attempt
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AttemptDetails(int id)
        {
            // Gets the logged-in user's ID
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            // Gets the attempt and its saved answers
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
            var challenge = await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c => c.ChallengeId == id);

            if (challenge == null)
            {
                return NotFound();
            }

            // Gets the logged-in user's ID
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            // Finds the connection between the user and challenge
            var userChallenge =
                await _challengeDbContext.UserChallenges
                    .FirstOrDefaultAsync(uc =>
                        uc.UserId == userId &&
                        uc.ChallengeId == id);

            // Creates UserChallenge if it does not already exist
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
            // If no questions were answered,
            // create an empty dictionary
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

            // Creates the result for the Result page
            var result = new ChallengeResultViewModel
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeTitle = challenge.Title,
                MaxScore =
                    challenge.Questions.Sum(q => q.Points)
            };

            // Gets the logged-in user's ID
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            // Finds the attempt that belongs to
            // the logged-in user and this challenge
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

                // Adds points if the answer is correct
                if (isCorrect)
                {
                    result.Score += question.Points;
                }

                // Adds the question to the Result page
                result.QuestionResults.Add(
                    new QuestionResultViewModel
                    {
                        Question = question,
                        SelectedOptionId =
                            selectedOptionId,
                        IsCorrect = isCorrect
                    });

                // Saves the answer in the attempt history
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

            // Shows the Result page
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