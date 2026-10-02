using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IChallengeRepository _challengeRepository;
        private readonly IAttemptRepository _attemptRepository;

        public QuestionController(
            IQuestionRepository questionRepository,
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository)
        {
            _questionRepository = questionRepository;
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
        }


        // Shows all questions - admin only.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Table()
        {
            List<Question> questions =
                await _questionRepository
                    .GetAllQuestionsAsync();

            return View(questions);
        }


        // Shows the Create Question form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(
            int? challengeId)
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

            // Admin can use all published challenges.
            // Normal users can only use their own challenges.
            List<Challenge> challenges;

            if (isAdmin)
            {
                challenges =
                    await _challengeRepository
                        .GetAllChallengesAsync();
            }
            else
            {
                challenges =
                    await _challengeRepository
                        .GetChallengesByUserIdAsync(userId);
            }

            var model =
                new QuestionFormViewModel
                {
                    Challenges = challenges
                };

            // Automatically selects the challenge
            // if we came from Challenge Details.
            if (challengeId.HasValue)
            {
                var challenge =
                    challenges.FirstOrDefault(c =>
                        c.ChallengeId ==
                        challengeId.Value);

                if (challenge == null)
                {
                    return Forbid();
                }

                model.Question.ChallengeId =
                    challengeId.Value;
            }

            return View(model);
        }


        // Creates a new question with answer options.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            QuestionFormViewModel model)
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

            var challenge =
                await _challengeRepository
                    .GetChallengeByIdAsync(
                        model.Question.ChallengeId);

            if (challenge == null)
            {
                return NotFound();
            }

            // Challenge is a navigation property
            // and is not submitted by the form.
            ModelState.Remove(
                "Question.Challenge");

            // Normal users can only add questions
            // to challenges they created.
            if (!isAdmin &&
                challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            // At least two answer options
            // must be filled in.
            int filledOptions =
                model.Options.Count(option =>
                    !string.IsNullOrWhiteSpace(option));

            if (filledOptions < 2)
            {
                ModelState.AddModelError(
                    "",
                    "You must enter at least two answer options.");
            }

            // The correct answer must be one
            // of the filled answer options.
            if (model.CorrectOption < 0 ||
                model.CorrectOption >= model.Options.Count ||
                string.IsNullOrWhiteSpace(
                    model.Options[model.CorrectOption]))
            {
                ModelState.AddModelError(
                    "",
                    "You must select a filled answer as the correct answer.");
            }

            if (ModelState.IsValid)
            {
                // The challenge content is changing,
                // so previous attempt history is reset.
                await _attemptRepository
                    .DeleteChallengeHistoryAsync(
                        model.Question.ChallengeId);

                var question =
                    model.Question;

                // Add the filled answer options.
                for (int i = 0;
                     i < model.Options.Count;
                     i++)
                {
                    // Empty options are not saved.
                    if (string.IsNullOrWhiteSpace(
                        model.Options[i]))
                    {
                        continue;
                    }

                    var option =
                        new QuestionOption
                        {
                            Text =
                                model.Options[i],

                            IsCorrect =
                                i == model.CorrectOption,

                            Question =
                                question
                        };

                    question.Options.Add(option);
                }

                await _questionRepository
                    .CreateQuestionAsync(question);

                TempData["SuccessMessage"] =
                    "Question created. Previous attempts and scores were reset.";

                // Return to the challenge
                // and show the question section.
                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new
                    {
                        id = question.ChallengeId,
                        showQuestions = true
                    });
            }

            // Reload challenges if validation fails.
            if (isAdmin)
            {
                model.Challenges =
                    await _challengeRepository
                        .GetAllChallengesAsync();
            }
            else
            {
                model.Challenges =
                    await _challengeRepository
                        .GetChallengesByUserIdAsync(userId);
            }

            return View(model);
        }


        // Shows the Update Question form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
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

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            // Normal users can only edit questions
            // belonging to their own challenges.
            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            var model =
                new QuestionFormViewModel
                {
                    Question = question,

                    Challenges =
                        new List<Challenge>
                        {
                            question.Challenge
                        }
                };

            // Get existing answer options.
            model.Options =
                question.Options
                    .Select(option => option.Text)
                    .ToList();

            // Find which option is correct
            // before adding empty option fields.
            var correctOption =
                question.Options
                    .Select((option, index) =>
                        new
                        {
                            option,
                            index
                        })
                    .FirstOrDefault(x =>
                        x.option.IsCorrect);

            if (correctOption != null)
            {
                model.CorrectOption =
                    correctOption.index;
            }

            // Always show four option fields.
            while (model.Options.Count < 4)
            {
                model.Options.Add("");
            }

            return View(model);
        }


        // Updates an existing question.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            QuestionFormViewModel model)
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

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(
                        model.Question.QuestionId);

            if (question == null)
            {
                return NotFound();
            }

            // Normal users can only edit questions
            // belonging to their own challenges.
            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            // Navigation property is not submitted
            // by the form.
            ModelState.Remove(
                "Question.Challenge");

            // At least two answer options
            // must be filled in.
            int filledOptions =
                model.Options.Count(option =>
                    !string.IsNullOrWhiteSpace(option));

            if (filledOptions < 2)
            {
                ModelState.AddModelError(
                    "",
                    "You must enter at least two answer options.");
            }

            // The selected correct answer
            // cannot be an empty option.
            if (model.CorrectOption < 0 ||
                model.CorrectOption >= model.Options.Count ||
                string.IsNullOrWhiteSpace(
                    model.Options[model.CorrectOption]))
            {
                ModelState.AddModelError(
                    "",
                    "You must select a filled answer as the correct answer.");
            }

            if (ModelState.IsValid)
            {
                // The challenge content is changing,
                // so previous attempt history is reset.
                await _attemptRepository
                    .DeleteChallengeHistoryAsync(
                        question.ChallengeId);

                // Update question information.
                question.Title =
                    model.Question.Title;

                question.Description =
                    model.Question.Description;

                question.Points =
                    model.Question.Points;

                question.ImageUrl =
                    model.Question.ImageUrl;

                // ChallengeId is deliberately
                // not changed here.

                // Replace the old answer options.
                question.Options.Clear();

                for (int i = 0;
                     i < model.Options.Count;
                     i++)
                {
                    // Empty options are not saved.
                    if (string.IsNullOrWhiteSpace(
                        model.Options[i]))
                    {
                        continue;
                    }

                    var option =
                        new QuestionOption
                        {
                            Text =
                                model.Options[i],

                            IsCorrect =
                                i == model.CorrectOption,

                            Question =
                                question
                        };

                    question.Options.Add(option);
                }

                await _questionRepository
                    .UpdateQuestionAsync(question);

                TempData["SuccessMessage"] =
                    "Question updated. Previous attempts and scores were reset.";

                // Return to the challenge
                // and show the question section.
                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new
                    {
                        id = question.ChallengeId,
                        showQuestions = true
                    });
            }

            // Reload the challenge
            // if validation fails.
            model.Question.ChallengeId =
                question.ChallengeId;

            model.Challenges =
                new List<Challenge>
                {
                    question.Challenge
                };

            // Always show four option fields.
            while (model.Options.Count < 4)
            {
                model.Options.Add("");
            }

            return View(model);
        }


        // Shows the Delete Question confirmation page.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
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

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            // Normal users can only delete questions
            // belonging to their own challenges.
            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            return View(question);
        }


        // Deletes the question.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
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

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            // Normal users can only delete questions
            // belonging to their own challenges.
            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                return Forbid();
            }

            int challengeId =
                question.ChallengeId;

            // A published challenge must always
            // contain at least one question.
            bool shouldUnpublish =
                question.Challenge.IsPublished &&
                question.Challenge.Questions.Count == 1;

            // The challenge content is changing,
            // so previous attempt history is reset.
            await _attemptRepository
                .DeleteChallengeHistoryAsync(
                    challengeId);

            if (shouldUnpublish)
            {
                question.Challenge.IsPublished = false;

                await _challengeRepository
                    .UpdateChallengeAsync(
                        question.Challenge);
            }

            await _questionRepository
                .DeleteQuestionAsync(question);

            if (shouldUnpublish)
            {
                TempData["SuccessMessage"] =
                    "Question deleted. The challenge was automatically unpublished because it has no questions left. Previous attempts and scores were reset.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Question deleted. Previous attempts and scores were reset.";
            }

            // Return to the challenge
            // and show the question section.
            return RedirectToAction(
                "Details",
                "Challenge",
                new
                {
                    id = challengeId,
                    showQuestions = true
                });
        }
    }
}