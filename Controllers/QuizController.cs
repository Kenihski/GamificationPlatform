using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class QuizController : Controller
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly ILogger<QuizController> _logger;


        public QuizController(
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            ILogger<QuizController> logger)
        {
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
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
                var correctOption =
                    answer.Question.Options
                        .FirstOrDefault(o =>
                            o.IsCorrect);

                var selectedOption =
                    answer.Question.Options
                        .FirstOrDefault(o =>
                            o.QuestionOptionId ==
                                answer.SelectedOptionId);

                var answerViewModel =
                    new AttemptAnswerViewModel
                    {
                        QuestionTitle =
                            answer.Question.Title,

                        QuestionDescription =
                            answer.Question.Description,

                        ImageUrl =
                            answer.Question.ImageUrl,

                        Points =
                            answer.Question.Points,

                        SelectedOptionId =
                            answer.SelectedOptionId,

                        IsCorrect =
                            selectedOption != null &&
                            selectedOption.IsCorrect,

                        CorrectAnswer =
                            correctOption?.Text
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
                                option.QuestionOptionId ==
                                answer.SelectedOptionId
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
            Dictionary<int, int>? answers)
        {
            var result =
                await CompleteAttemptAsync(
                    challengeId,
                    challengeAttemptId,
                    answers);

            if (result == null)
            {
                return NotFound();
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
            Dictionary<int, int>? answers)
        {
            var result =
                await CompleteAttemptAsync(
                    challengeId,
                    challengeAttemptId,
                    answers);

            if (result == null)
            {
                return NotFound();
            }

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
                Dictionary<int, int>? answers)
        {
            answers ??=
                new Dictionary<int, int>();


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
                    answers.Clear();
                }
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
                    result.Score +=
                        question.Points;
                }


                result.QuestionResults.Add(
                    new QuestionResultViewModel
                    {
                        Question = question,

                        SelectedOptionId =
                            selectedOptionId,

                        IsCorrect = isCorrect
                    });


                // An unanswered question is also saved,
                // but without a selected option.
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


            _logger.LogInformation(
                "User {UserId} completed attempt {AttemptId} for challenge {ChallengeId} with score {Score}/{MaxScore}.",
                userId,
                challengeAttemptId,
                challengeId,
                result.Score,
                result.MaxScore);


            return result;
        }
    }
}