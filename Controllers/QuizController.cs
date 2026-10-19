
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using GamificationPlatform.Services;

namespace GamificationPlatform.Controllers
{
    public class QuizController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            IAchievementService achievementService,
            ILogger<QuizController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _achievementService = achievementService;
            _logger = logger;
        }

        // Shows the results of a completed attempt.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AttemptDetails(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");

            var attempt = await _attemptRepository
                .GetAttemptDetailsAsync(id);

            if (attempt == null)
            {
                return NotFound();
            }

            // Users can only view their own attempts.
            if (!isAdmin &&
                attempt.UserChallenge.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to view another user's attempt {AttemptId}.",
                    userId,
                    id);

                return Forbid();
            }

            var challenge = attempt.UserChallenge.Challenge;

            bool showAnswers =
                challenge.ShowAnswersAfterCompletion ||
                challenge.CreatedByUserId == userId ||
                isAdmin;

            var viewModel = new AttemptDetailsViewModel
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeTitle = challenge.Title,
                Score = attempt.Score,
                MaxPoints = challenge.Questions.Sum(q => q.Points),
                ShowAnswers = showAnswers,
                CompletedAt = attempt.CompletedAt?
                    .ToString("dd.MM.yyyy HH:mm")
            };

            if (attempt.CompletedAt.HasValue)
            {
                var timeUsed =
                    attempt.CompletedAt.Value - attempt.StartedAt;

                int totalSeconds = Math.Max(
                    0,
                    (int)Math.Round(timeUsed.TotalSeconds));

                viewModel.TimeUsed =
                    $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
            }

            foreach (var answer in attempt.Answers)
            {
                var question = answer.Question;

                var selectedOptionIds = answer.SelectedOptions
                    .Select(selected => selected.QuestionOptionId)
                    .ToList();

                bool isCorrect;
                List<string> correctAnswers;

                if (question.QuestionType == "ShortAnswer")
                {
                    isCorrect = IsAcceptedShortAnswer(
                        answer.TextAnswer,
                        question.AcceptedAnswers);

                    correctAnswers = question.AcceptedAnswers
                        .Select(accepted => accepted.Text)
                        .ToList();
                }
                else
                {
                    var correctOptionIds = question.Options
                        .Where(option => option.IsCorrect)
                        .Select(option => option.QuestionOptionId)
                        .ToList();

                    isCorrect =
                        selectedOptionIds.Count > 0 &&
                        selectedOptionIds
                            .OrderBy(optionId => optionId)
                            .SequenceEqual(
                                correctOptionIds.OrderBy(optionId => optionId));

                    correctAnswers = question.Options
                        .Where(option => option.IsCorrect)
                        .Select(option => option.Text)
                        .ToList();
                }

                var answerViewModel = new AttemptAnswerViewModel
                {
                    QuestionTitle = question.Title,
                    QuestionDescription =
                        question.Description ?? string.Empty,
                    QuestionType = question.QuestionType,
                    ImageUrl = question.ImageUrl,
                    Points = question.Points,
                    PointsEarned = isCorrect ? question.Points : 0,
                    IsCorrect = isCorrect
                };

                // Only include sensitive answer data when allowed.
                if (showAnswers)
                {
                    answerViewModel.QuestionExplanation =
                        question.Explanation ?? string.Empty;

                    answerViewModel.SelectedOptionIds =
                        selectedOptionIds;

                    answerViewModel.TextAnswer =
                        answer.TextAnswer;

                    answerViewModel.CorrectAnswers =
                        correctAnswers;

                    foreach (var option in question.Options)
                    {
                        answerViewModel.Options.Add(
                            new AttemptOptionViewModel
                            {
                                QuestionOptionId = option.QuestionOptionId,
                                Text = option.Text,
                                IsSelected = selectedOptionIds.Contains(
                                    option.QuestionOptionId),
                                IsCorrect = option.IsCorrect
                            });
                    }
                }

                viewModel.Answers.Add(answerViewModel);
            }

            return View(viewModel);
        }

        // Shows the challenge and its questions.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Take(int id)
        {
            var challenge = await _challengeRepository
                .GetChallengeWithQuestionsAndOptionsAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            bool isOwner = challenge.CreatedByUserId == userId;
            bool isAdmin = User.IsInRole("Admin");

            // Only owners and admins can take unpublished challenges.
            if (!challenge.IsPublished && !isOwner && !isAdmin)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to start unpublished challenge {ChallengeId} without permission.",
                    userId,
                    id);

                TempData["ErrorMessage"] =
                    "This challenge is not published and cannot be started.";

                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new { id = challenge.ChallengeId });
            }

            if (!challenge.Questions.Any())
            {
                TempData["ErrorMessage"] =
                    "The challenge must have at least one question before it can be started.";

                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new { id = challenge.ChallengeId });
            }

            var userChallenge = await _attemptRepository
                .GetUserChallengeAsync(userId, id);

            if (userChallenge == null)
            {
                userChallenge = new UserChallenge
                {
                    UserId = userId,
                    ChallengeId = id
                };

                await _attemptRepository
                    .CreateUserChallengeAsync(userChallenge);
            }

            var challengeAttempt = await _attemptRepository
                .GetUnfinishedAttemptAsync(
                    userChallenge.UserChallengeId);

            if (challengeAttempt == null)
            {
                challengeAttempt = new ChallengeAttempt
                {
                    UserChallengeId = userChallenge.UserChallengeId,
                    Score = 0,
                    Completed = false,
                    StartedAt = DateTime.Now
                };

                await _attemptRepository
                    .CreateAttemptAsync(challengeAttempt);

                _logger.LogInformation(
                    "User {UserId} started attempt {AttemptId} for challenge {ChallengeId}.",
                    userId,
                    challengeAttempt.ChallengeAttemptId,
                    id);
            }

            var viewModel = new TakeChallengeViewModel
            {
                Challenge = challenge,
                ChallengeAttemptId =
                    challengeAttempt.ChallengeAttemptId,
                StartedAt = challengeAttempt.StartedAt
            };

            return View(viewModel);
        }

        // Submits and completes the challenge attempt.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int challengeId,
            int challengeAttemptId,
            [FromForm(Name = "answers")]
            Dictionary<int, List<int>>? answers,
            [FromForm(Name = "textAnswers")]
            Dictionary<int, string>? textAnswers)
        {
            if (!ModelState.IsValid ||
                challengeId <= 0 ||
                challengeAttemptId <= 0)
            {
                _logger.LogWarning(
                    "Quiz submission rejected because the submitted fields were invalid.");

                return BadRequest();
            }

            var result = await CompleteAttemptAsync(
                challengeId,
                challengeAttemptId,
                answers,
                textAnswers);

            if (result == null)
            {
                return ModelState.IsValid
                    ? NotFound()
                    : BadRequest();
            }

            return View("Result", result);
        }

        // Ends the challenge early and returns to challenge details.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Exit(
            int challengeId,
            int challengeAttemptId,
            [FromForm(Name = "answers")]
            Dictionary<int, List<int>>? answers,
            [FromForm(Name = "textAnswers")]
            Dictionary<int, string>? textAnswers)
        {
            if (!ModelState.IsValid ||
                challengeId <= 0 ||
                challengeAttemptId <= 0)
            {
                _logger.LogWarning(
                    "Quiz submission rejected because the submitted fields were invalid.");

                return BadRequest();
            }

            var result = await CompleteAttemptAsync(
                challengeId,
                challengeAttemptId,
                answers,
                textAnswers);

            if (result == null)
            {
                return ModelState.IsValid
                    ? NotFound()
                    : BadRequest();
            }

            _logger.LogInformation(
                "Attempt {AttemptId} for challenge {ChallengeId} was ended through Exit Challenge.",
                challengeAttemptId,
                challengeId);

            TempData["SuccessMessage"] =
                $"Challenge ended. Your attempt was submitted with {result.Score} / {result.MaxScore} points.";

            return RedirectToAction(
                "Details",
                "Challenge",
                new { id = challengeId });
        }

        // Checks the answers, calculates the score
        // and completes the current attempt.
        private async Task<ChallengeResultViewModel?> CompleteAttemptAsync(
            int challengeId,
            int challengeAttemptId,
            Dictionary<int, List<int>>? answers,
            Dictionary<int, string>? textAnswers)
        {
            answers ??= new Dictionary<int, List<int>>();
            textAnswers ??= new Dictionary<int, string>();

            var challenge = await _challengeRepository
                .GetChallengeWithQuestionsAndOptionsAsync(challengeId);

            if (challenge == null)
            {
                return null;
            }

            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return null;
            }

            bool isOwner = challenge.CreatedByUserId == userId;
            bool isAdmin = User.IsInRole("Admin");

            if (!challenge.IsPublished && !isOwner && !isAdmin)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to submit unpublished challenge {ChallengeId} without permission.",
                    userId,
                    challengeId);

                ModelState.AddModelError(
                    "challengeId",
                    "You do not have permission to submit this challenge.");

                return null;
            }

            if (!challenge.Questions.Any())
            {
                ModelState.AddModelError(
                    "challengeId",
                    "A challenge must contain at least one question.");

                return null;
            }

            bool showAnswers =
                challenge.ShowAnswersAfterCompletion ||
                isOwner ||
                isAdmin;

            var result = new ChallengeResultViewModel
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeTitle = challenge.Title,
                MaxScore = challenge.Questions.Sum(q => q.Points),
                ShowAnswers = showAnswers
            };

            var challengeAttempt = await _attemptRepository
                .GetValidUnfinishedAttemptAsync(
                    challengeAttemptId,
                    userId,
                    challengeId);

            if (challengeAttempt == null)
            {
                _logger.LogWarning(
                    "Submission failed for challenge {ChallengeId}: attempt {AttemptId} was not valid for user {UserId}.",
                    challengeId,
                    challengeAttemptId,
                    userId);

                return null;
            }

            // Validate submitted choice answers.
            foreach (var answer in answers)
            {
                var question = challenge.Questions
                    .FirstOrDefault(q =>
                        q.QuestionId == answer.Key);

                if (question == null ||
                    question.QuestionType == "ShortAnswer")
                {
                    _logger.LogWarning(
                        "User {UserId} submitted an invalid choice answer for question {QuestionId} in attempt {AttemptId}.",
                        userId,
                        answer.Key,
                        challengeAttemptId);

                    ModelState.AddModelError(
                        "answers",
                        "An answer does not belong to this challenge question.");

                    return null;
                }

                var submittedOptionIds = answer.Value?
                    .Distinct()
                    .ToList()
                    ?? new List<int>();

                bool containsInvalidOption =
                    submittedOptionIds.Any(optionId =>
                        !question.Options.Any(option =>
                            option.QuestionOptionId == optionId));

                if (containsInvalidOption)
                {
                    _logger.LogWarning(
                        "User {UserId} submitted an invalid option for question {QuestionId} in attempt {AttemptId}.",
                        userId,
                        answer.Key,
                        challengeAttemptId);

                    ModelState.AddModelError(
                        "answers",
                        "An answer does not belong to this challenge question.");

                    return null;
                }

                if (question.QuestionType == "SingleChoice" &&
                    submittedOptionIds.Count > 1)
                {
                    _logger.LogWarning(
                        "User {UserId} submitted multiple options for single choice question {QuestionId}.",
                        userId,
                        question.QuestionId);

                    ModelState.AddModelError(
                        "answers",
                        "A single choice question can only have one selected answer.");

                    return null;
                }
            }

            // Validate submitted short answers.
            foreach (var textAnswer in textAnswers)
            {
                var question = challenge.Questions
                    .FirstOrDefault(q =>
                        q.QuestionId == textAnswer.Key);

                if (question == null ||
                    question.QuestionType != "ShortAnswer")
                {
                    _logger.LogWarning(
                        "User {UserId} submitted an invalid text answer for question {QuestionId} in attempt {AttemptId}.",
                        userId,
                        textAnswer.Key,
                        challengeAttemptId);

                    ModelState.AddModelError(
                        "textAnswers",
                        "A text answer does not belong to this challenge question.");

                    return null;
                }

                if (textAnswer.Value != null &&
                    textAnswer.Value.Length > 200)
                {
                    ModelState.AddModelError(
                        "textAnswers",
                        "A short answer cannot be longer than 200 characters.");

                    return null;
                }
            }

            // Enforce the time limit on the server.
            if (challenge.TimeLimitMinutes.HasValue)
            {
                DateTime timeLimit = challengeAttempt.StartedAt
                    .AddMinutes(challenge.TimeLimitMinutes.Value);

                if (DateTime.Now > timeLimit.AddSeconds(5))
                {
                    _logger.LogWarning(
                        "Attempt {AttemptId} was submitted after the time limit; answers were ignored.",
                        challengeAttemptId);

                    answers.Clear();
                    textAnswers.Clear();
                }
            }

            foreach (var question in challenge.Questions)
            {
                var selectedOptionIds = new List<int>();
                string? submittedTextAnswer = null;
                bool isCorrect = false;

                if (question.QuestionType == "ShortAnswer")
                {
                    if (textAnswers.TryGetValue(
                        question.QuestionId,
                        out var textAnswer))
                    {
                        submittedTextAnswer =
                            string.IsNullOrWhiteSpace(textAnswer)
                                ? null
                                : textAnswer.Trim();
                    }

                    isCorrect = IsAcceptedShortAnswer(
                        submittedTextAnswer,
                        question.AcceptedAnswers);
                }
                else
                {
                    if (answers.TryGetValue(
                        question.QuestionId,
                        out var submittedOptionIds) &&
                        submittedOptionIds != null)
                    {
                        selectedOptionIds = submittedOptionIds
                            .Distinct()
                            .ToList();
                    }

                    var correctOptionIds = question.Options
                        .Where(option => option.IsCorrect)
                        .Select(option => option.QuestionOptionId)
                        .ToList();

                    isCorrect =
                        selectedOptionIds.Count > 0 &&
                        selectedOptionIds
                            .OrderBy(id => id)
                            .SequenceEqual(
                                correctOptionIds.OrderBy(id => id));
                }

                if (isCorrect)
                {
                    result.Score += question.Points;
                }

                // Do not expose correct answers when review is disabled.
                var resultQuestion = showAnswers
                    ? question
                    : new Question
                    {
                        QuestionId = question.QuestionId,
                        Title = question.Title,
                        Description = question.Description,
                        QuestionType = question.QuestionType,
                        ImageUrl = question.ImageUrl,
                        Points = question.Points
                    };

                result.QuestionResults.Add(
                    new QuestionResultViewModel
                    {
                        Question = resultQuestion,
                        SelectedOptionIds = showAnswers
                            ? selectedOptionIds
                            : new List<int>(),
                        TextAnswer = showAnswers
                            ? submittedTextAnswer
                            : null,
                        IsCorrect = isCorrect,
                        PointsEarned = isCorrect ? question.Points : 0
                    });

                // Save answers regardless of review settings.
                var attemptAnswer = new AttemptAnswer
                {
                    ChallengeAttemptId =
                        challengeAttempt.ChallengeAttemptId,
                    QuestionId = question.QuestionId,
                    TextAnswer = submittedTextAnswer
                };

                foreach (int selectedOptionId in selectedOptionIds)
                {
                    attemptAnswer.SelectedOptions.Add(
                        new AttemptAnswerOption
                        {
                            QuestionOptionId = selectedOptionId
                        });
                }

                _attemptRepository.AddAttemptAnswer(attemptAnswer);
            }

            challengeAttempt.Score = result.Score;
            challengeAttempt.Completed = true;
            challengeAttempt.CompletedAt = DateTime.Now;

            result.CompletedAt = challengeAttempt.CompletedAt;

            result.TimeUsed =
                challengeAttempt.CompletedAt.Value -
                challengeAttempt.StartedAt;

            await _attemptRepository.SaveChangesAsync();

            // Recheck achievements after completion.
            var users = await _userRepository.GetAllUsersAsync();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    continue;
                }

                await _achievementService
                    .CheckAchievementsAsync(user.Id);
            }

            _logger.LogInformation(
                "User {UserId} completed attempt {AttemptId} for challenge {ChallengeId} with score {Score}/{MaxScore}.",
                userId,
                challengeAttemptId,
                challengeId,
                result.Score,
                result.MaxScore);

            return result;
        }

        // Normalizes a short answer before comparison.
        private static string NormalizeShortAnswer(string answer)
        {
            return string.Join(
                " ",
                answer
                    .Trim()
                    .ToLowerInvariant()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries));
        }

        // Checks whether a short answer is accepted.
        private static bool IsAcceptedShortAnswer(
            string? submittedAnswer,
            IEnumerable<QuestionAcceptedAnswer> acceptedAnswers)
        {
            if (string.IsNullOrWhiteSpace(submittedAnswer))
            {
                return false;
            }

            string normalizedSubmittedAnswer =
                NormalizeShortAnswer(submittedAnswer);

            return acceptedAnswers.Any(acceptedAnswer =>
                NormalizeShortAnswer(acceptedAnswer.Text) ==
                normalizedSubmittedAnswer);
        }
    }
}
