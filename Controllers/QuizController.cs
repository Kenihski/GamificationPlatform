using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IAchievementService _achievementService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            IUserRepository userRepository,
            IAchievementService achievementService,
            ILogger<QuizController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _userRepository = userRepository;
            _achievementService = achievementService;
            _logger = logger;
        }

        // Shows the answers from a completed attempt.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AttemptDetails(int id)
        {
            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            bool isAdmin =
                User.IsInRole("Admin");

            var attempt =
                await _attemptRepository
                    .GetAttemptDetailsAsync(id);

            if (attempt == null)
            {
                return NotFound();
            }

            // Normal users can only view
            // their own attempts.
            // Admins can view all attempts.
            if (!isAdmin &&
                attempt.UserChallenge.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to view another user's attempt {AttemptId}.",
                    userId,
                    id);

                return Forbid();
            }

            var challenge =
                attempt.UserChallenge.Challenge;

            // Prepares the attempt information
            // needed by the view.
            var viewModel =
                new AttemptDetailsViewModel
                {
                    ChallengeId =
                        challenge.ChallengeId,

                    ChallengeTitle =
                        challenge.Title,

                    Score =
                        attempt.Score,

                    MaxPoints =
                        challenge.Questions
                            .Sum(q => q.Points),

                    CompletedAt =
                        attempt.CompletedAt?
                            .ToString("dd.MM.yyyy HH:mm")
                };

            // Calculates how long the attempt took.
            if (attempt.CompletedAt.HasValue)
            {
                var timeUsed =
                    attempt.CompletedAt.Value -
                    attempt.StartedAt;

                if (timeUsed.TotalMinutes >= 1)
                {
                    viewModel.TimeUsed =
                        $"{Math.Round(timeUsed.TotalMinutes)} min";
                }
                else
                {
                    viewModel.TimeUsed =
                        $"{Math.Round(timeUsed.TotalSeconds)} sec";
                }
            }

            // Prepares the answers for the view.
            foreach (var answer in attempt.Answers)
            {
                var selectedOptionIds =
                    answer.SelectedOptions
                        .Select(selected =>
                            selected.QuestionOptionId)
                        .ToList();

                bool isCorrect;

                List<string> correctAnswers;

                if (answer.Question.QuestionType ==
                    "ShortAnswer")
                {
                    isCorrect =
                        IsAcceptedShortAnswer(
                            answer.TextAnswer,
                            answer.Question.AcceptedAnswers);

                    correctAnswers =
                        answer.Question.AcceptedAnswers
                            .Select(accepted =>
                                accepted.Text)
                            .ToList();
                }
                else
                {
                    var correctOptionIds =
                        answer.Question.Options
                            .Where(option =>
                                option.IsCorrect)
                            .Select(option =>
                                option.QuestionOptionId)
                            .ToList();

                    isCorrect =
                        selectedOptionIds.Count > 0 &&
                        selectedOptionIds
                            .OrderBy(optionId =>
                                optionId)
                            .SequenceEqual(
                                correctOptionIds
                                    .OrderBy(optionId =>
                                        optionId));

                    correctAnswers =
                        answer.Question.Options
                            .Where(option =>
                                option.IsCorrect)
                            .Select(option =>
                                option.Text)
                            .ToList();
                }

                var answerViewModel =
                    new AttemptAnswerViewModel
                    {
                        QuestionTitle =
                            answer.Question.Title,

                        QuestionDescription =
                            answer.Question.Description ?? string.Empty,

                        QuestionExplanation =
                            answer.Question.Explanation ?? string.Empty,

                        QuestionType =
                            answer.Question.QuestionType,

                        ImageUrl =
                            answer.Question.ImageUrl,

                        Points =
                            answer.Question.Points,

                        SelectedOptionIds =
                            selectedOptionIds,

                        TextAnswer =
                            answer.TextAnswer,

                        IsCorrect =
                            isCorrect,

                        CorrectAnswers =
                            correctAnswers
                    };

                foreach (var option in
                    answer.Question.Options)
                {
                    answerViewModel.Options.Add(
                        new AttemptOptionViewModel
                        {
                            QuestionOptionId =
                                option.QuestionOptionId,

                            Text =
                                option.Text,

                            IsSelected =
                                selectedOptionIds.Contains(
                                    option.QuestionOptionId)
                        });
                }

                viewModel.Answers.Add(
                    answerViewModel);
            }

            return View(viewModel);
        }

        // Shows the challenge and its questions.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Take(int id)
        {
            var challenge =
                await _challengeRepository
                    .GetChallengeWithQuestionsAndOptionsAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            // Draft challenges cannot be taken.
            // Sends the user back to the challenge
            // instead of showing a 404 page.
            if (!challenge.IsPublished)
            {
                TempData["ErrorMessage"] =
                    "This challenge is not published and cannot be started.";

                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new
                    {
                        id = challenge.ChallengeId
                    });
            }

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var userChallenge =
                await _attemptRepository
                    .GetUserChallengeAsync(
                        userId,
                        id);

            if (userChallenge == null)
            {
                userChallenge =
                    new UserChallenge
                    {
                        UserId = userId,
                        ChallengeId = id
                    };

                await _attemptRepository
                    .CreateUserChallengeAsync(
                        userChallenge);
            }

            // Gets an unfinished attempt
            // if one already exists.
            var challengeAttempt =
                await _attemptRepository
                    .GetUnfinishedAttemptAsync(
                        userChallenge.UserChallengeId);

            // Creates a new attempt if there
            // is no unfinished attempt.
            if (challengeAttempt == null)
            {
                challengeAttempt =
                    new ChallengeAttempt
                    {
                        UserChallengeId =
                            userChallenge.UserChallengeId,

                        Score = 0,
                        Completed = false,
                        StartedAt = DateTime.Now
                    };

                await _attemptRepository
                    .CreateAttemptAsync(
                        challengeAttempt);

                _logger.LogInformation(
                    "User {UserId} started attempt {AttemptId} for challenge {ChallengeId}.",
                    userId,
                    challengeAttempt.ChallengeAttemptId,
                    id);
            }

            var viewModel =
                new TakeChallengeViewModel
                {
                    Challenge = challenge,

                    ChallengeAttemptId =
                        challengeAttempt.ChallengeAttemptId,

                    StartedAt =
                        challengeAttempt.StartedAt
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

            var result =
                await CompleteAttemptAsync(
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

            return View(
                "Result",
                result);
        }

        // Ends the challenge early and returns
        // the user to the challenge details page.
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

            var result =
                await CompleteAttemptAsync(
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
                new
                {
                    id = challengeId
                });
        }

        // Checks the answers, calculates the score
        // and completes the current attempt.
        private async Task<ChallengeResultViewModel?>
            CompleteAttemptAsync(
                int challengeId,
                int challengeAttemptId,
                Dictionary<int, List<int>>? answers,
                Dictionary<int, string>? textAnswers)
        {
            answers ??=
                new Dictionary<int, List<int>>();

            textAnswers ??=
                new Dictionary<int, string>();

            var challenge =
                await _challengeRepository
                    .GetChallengeWithQuestionsAndOptionsAsync(
                        challengeId);

            if (challenge == null)
            {
                return null;
            }

            var result =
                new ChallengeResultViewModel
                {
                    ChallengeId =
                        challenge.ChallengeId,

                    ChallengeTitle =
                        challenge.Title,

                    MaxScore =
                        challenge.Questions
                            .Sum(q => q.Points)
                };

            var userIdString =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return null;
            }

            int userId =
                int.Parse(userIdString);

            var challengeAttempt =
                await _attemptRepository
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

            // Validate that every submitted choice answer
            // belongs to the submitted question.
            foreach (var answer in answers)
            {
                var question =
                    challenge.Questions
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

                var submittedOptionIds =
                    answer.Value?
                        .Distinct()
                        .ToList()
                    ?? new List<int>();

                bool containsInvalidOption =
                    submittedOptionIds.Any(optionId =>
                        !question.Options.Any(option =>
                            option.QuestionOptionId ==
                                optionId));

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

                // Single Choice must never contain
                // more than one selected option.
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

            // Validate that every submitted text answer
            // belongs to a Short Answer question.
            foreach (var textAnswer in textAnswers)
            {
                var question =
                    challenge.Questions
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

            // Checks the time limit on the server.
            if (challenge.TimeLimitMinutes.HasValue)
            {
                DateTime timeLimit =
                    challengeAttempt.StartedAt
                        .AddMinutes(
                            challenge.TimeLimitMinutes.Value);

                // Allows a few seconds for the automatic
                // form submission to reach the server.
                if (DateTime.Now >
                    timeLimit.AddSeconds(5))
                {
                    _logger.LogWarning(
                        "Attempt {AttemptId} was submitted after the time limit; answers were ignored.",
                        challengeAttemptId);

                    answers.Clear();
                    textAnswers.Clear();
                }
            }

            foreach (var question in
                challenge.Questions)
            {
                var selectedOptionIds =
                    new List<int>();

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

                    isCorrect =
                        IsAcceptedShortAnswer(
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
                        selectedOptionIds =
                            submittedOptionIds
                                .Distinct()
                                .ToList();
                    }

                    var correctOptionIds =
                        question.Options
                            .Where(option =>
                                option.IsCorrect)
                            .Select(option =>
                                option.QuestionOptionId)
                            .ToList();

                    // The answer is correct only when
                    // all correct options and no incorrect
                    // options were selected.
                    isCorrect =
                        selectedOptionIds.Count > 0 &&
                        selectedOptionIds
                            .OrderBy(id => id)
                            .SequenceEqual(
                                correctOptionIds
                                    .OrderBy(id => id));
                }

                if (isCorrect)
                {
                    result.Score +=
                        question.Points;
                }

                result.QuestionResults.Add(
                    new QuestionResultViewModel
                    {
                        Question = question,

                        SelectedOptionIds =
                            selectedOptionIds,

                        TextAnswer =
                            submittedTextAnswer,

                        IsCorrect =
                            isCorrect
                    });

                // An unanswered question is also saved.
                var attemptAnswer =
                    new AttemptAnswer
                    {
                        ChallengeAttemptId =
                            challengeAttempt
                                .ChallengeAttemptId,

                        QuestionId =
                            question.QuestionId,

                        TextAnswer =
                            submittedTextAnswer
                    };

                foreach (int selectedOptionId in
                    selectedOptionIds)
                {
                    attemptAnswer.SelectedOptions.Add(
                        new AttemptAnswerOption
                        {
                            QuestionOptionId =
                                selectedOptionId
                        });
                }

                _attemptRepository
                    .AddAttemptAnswer(
                        attemptAnswer);
            }

            // Saves the completed attempt.
            challengeAttempt.Score =
                result.Score;

            challengeAttempt.Completed =
                true;

            challengeAttempt.CompletedAt =
                DateTime.Now;

            // Calculates how long the user
            // spent on the challenge.
            result.TimeUsed =
                challengeAttempt.CompletedAt.Value -
                challengeAttempt.StartedAt;

            // Saves all answers and the completed
            // attempt together.
            await _attemptRepository
                .SaveChangesAsync();

            // A completed attempt can change leaderboard
            // positions, so achievements are checked
            // for all normal users.
            var users =
                await _userRepository
                    .GetAllUsersAsync();

            foreach (var user in
                users.Where(u => !u.IsAdmin))
            {
                await _achievementService
                    .CheckAchievementsAsync(
                        user.UserId);
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
        private static string NormalizeShortAnswer(
            string answer)
        {
            return string.Join(
                " ",
                answer
                    .Trim()
                    .ToLowerInvariant()
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries));
        }

        // Checks whether the submitted text matches
        // one of the accepted answers.
        private static bool IsAcceptedShortAnswer(
            string? submittedAnswer,
            IEnumerable<QuestionAcceptedAnswer> acceptedAnswers)
        {
            if (string.IsNullOrWhiteSpace(
                submittedAnswer))
            {
                return false;
            }

            string normalizedSubmittedAnswer =
                NormalizeShortAnswer(
                    submittedAnswer);

            return acceptedAnswers.Any(
                acceptedAnswer =>
                    NormalizeShortAnswer(
                        acceptedAnswer.Text) ==
                    normalizedSubmittedAnswer);
        }
    }
}