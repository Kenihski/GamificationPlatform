
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserImageRepository _userImageRepository;
        private readonly ChallengeDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(
            IQuestionRepository questionRepository,
            IChallengeRepository challengeRepository,
            IAttemptRepository attemptRepository,
            IUserImageRepository userImageRepository,
            ChallengeDbContext context,
            IWebHostEnvironment environment,
            ILogger<QuestionController> logger)
        {
            _questionRepository = questionRepository;
            _challengeRepository = challengeRepository;
            _attemptRepository = attemptRepository;
            _userImageRepository = userImageRepository;
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // Shows all questions - admin only.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Table()
        {
            List<Question> questions =
                await _questionRepository.GetAllQuestionsAsync();

            return View(questions);
        }

        // Loads images for the Question Create and Update forms.
        // Topic images appear alphabetically first.
        // Numbered Question images appear last, in numerical order.
        private async Task LoadImagesAsync(int userId)
        {
            ViewBag.MyImages =
                await _userImageRepository.GetImagesByUserIdAsync(userId);

            var defaultImages = await _context.DefaultImages
                .ToListAsync();

            ViewData["DefaultImages"] = defaultImages
                .OrderBy(image =>
                    image.Name.StartsWith(
                        "Question ",
                        StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(image.Name.Substring(9), out _)
                        ? 1
                        : 0)
                .ThenBy(image =>
                    image.Name.StartsWith(
                        "Question ",
                        StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(image.Name.Substring(9), out int number)
                        ? number
                        : 0)
                .ThenBy(image => image.Name)
                .ToList();
        }

        // Checks that the selected image is available to the user.
        private async Task<bool> IsAllowedImageAsync(
            string? imageUrl,
            int userId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true;
            }

            bool isDefaultImage = await _context.DefaultImages
                .AnyAsync(image => image.ImageUrl == imageUrl);

            if (isDefaultImage)
            {
                return true;
            }

            var images =
                await _userImageRepository.GetImagesByUserIdAsync(userId);

            return images.Any(image => image.ImageUrl == imageUrl);
        }

        // Validates and saves an uploaded image to the user's library.
        private async Task<string?> SaveImageAsync(
            IFormFile file,
            int userId)
        {
            const long maxSize = 5 * 1024 * 1024;

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (file.Length == 0 ||
                file.Length > maxSize ||
                !new[] { ".jpg", ".jpeg", ".png", ".webp" }
                    .Contains(extension))
            {
                ModelState.AddModelError(
                    "imageUpload",
                    "Choose a JPG, PNG or WebP image up to 5 MB.");

                return null;
            }

            // Check the file signature.
            byte[] header = new byte[12];

            using (var stream = file.OpenReadStream())
            {
                int count = await stream.ReadAsync(header.AsMemory());

                bool jpeg =
                    count >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                bool png =
                    count >= 8 &&
                    header.AsSpan(0, 8).SequenceEqual(
                        new byte[]
                        {
                            137, 80, 78, 71, 13, 10, 26, 10
                        });

                bool webp =
                    count >= 12 &&
                    System.Text.Encoding.ASCII.GetString(
                        header, 0, 4) == "RIFF" &&
                    System.Text.Encoding.ASCII.GetString(
                        header, 8, 4) == "WEBP";

                bool valid = extension switch
                {
                    ".jpg" or ".jpeg" => jpeg,
                    ".png" => png,
                    ".webp" => webp,
                    _ => false
                };

                if (!valid)
                {
                    ModelState.AddModelError(
                        "imageUpload",
                        "The file does not match a supported image format.");

                    return null;
                }
            }

            string fileName = $"{Guid.NewGuid():N}{extension}";

            string folder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "images");

            Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, fileName);
            string url = $"/uploads/images/{fileName}";

            try
            {
                await using (var output =
                    new FileStream(fullPath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(output);
                }

                await _userImageRepository.AddImageAsync(
                    new UserImage
                    {
                        UserId = userId,
                        FileName = Path.GetFileName(file.FileName),
                        ImageUrl = url
                    });

                return url;
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                throw;
            }
        }

        // Shows the Create Question form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int? challengeId)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            List<Challenge> challenges;

            if (isAdmin)
            {
                challenges =
                    await _challengeRepository.GetAllChallengesAsync();
            }
            else
            {
                challenges =
                    await _challengeRepository.GetChallengesByUserIdAsync(userId);
            }

            var model = new QuestionFormViewModel
            {
                Challenges = challenges
            };

            // Automatically selects the challenge when coming
            // from Challenge Details.
            if (challengeId.HasValue)
            {
                var challenge = challenges.FirstOrDefault(c =>
                    c.ChallengeId == challengeId.Value);

                if (challenge == null)
                {
                    _logger.LogWarning(
                        "User {UserId} was denied question management access.",
                        userId);

                    return Forbid();
                }

                model.Question.ChallengeId = challengeId.Value;
            }

            await LoadImagesAsync(userId);

            return View(model);
        }

        // Creates a new question.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            QuestionFormViewModel model,
            IFormFile? imageUpload)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (model.Question == null)
            {
                _logger.LogWarning(
                    "Question creation rejected because the question data was missing.");

                return BadRequest();
            }

            var challenge =
                await _challengeRepository.GetChallengeByIdAsync(
                    model.Question.ChallengeId);

            if (challenge == null)
            {
                return NotFound();
            }

            // Navigation property is not submitted by the form.
            ModelState.Remove("Question.Challenge");

            // ImageUrl may be empty when uploading a new image
            // or choosing No image.
            ModelState.Remove("Question.ImageUrl");

            // Normal users can only add questions to their own challenges.
            if (!isAdmin && challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied question management access.",
                    userId);

                return Forbid();
            }

            // An uploaded image and an existing image
            // cannot be selected at the same time.
            if (imageUpload != null &&
                !string.IsNullOrWhiteSpace(model.Question.ImageUrl))
            {
                ModelState.AddModelError(
                    "Question.ImageUrl",
                    "Choose either an existing image or an upload.");
            }

            if (!await IsAllowedImageAsync(
                model.Question.ImageUrl, userId))
            {
                ModelState.AddModelError(
                    "Question.ImageUrl",
                    "This image is not available to you.");
            }

            ValidateAnswers(model);

            if (ModelState.IsValid && imageUpload != null)
            {
                string? url = await SaveImageAsync(imageUpload, userId);

                if (url != null)
                {
                    model.Question.ImageUrl = url;
                }
            }

            if (ModelState.IsValid)
            {
                // The challenge content is changing,
                // so previous attempt history is reset.
                await _attemptRepository.DeleteChallengeHistoryAsync(
                    model.Question.ChallengeId);

                // Copy only editable fields.
                var question = new Question
                {
                    Title = model.Question.Title,
                    Description = model.Question.Description,
                    Explanation = model.Question.Explanation,
                    Points = model.Question.Points,
                    ImageUrl = model.Question.ImageUrl ?? string.Empty,
                    QuestionType = model.Question.QuestionType,
                    ChallengeId = challenge.ChallengeId
                };

                if (question.QuestionType == "ShortAnswer")
                {
                    AddAcceptedAnswers(
                        question,
                        model.AcceptedAnswers);
                }
                else
                {
                    AddAnswerOptions(
                        question,
                        model.Options,
                        model.CorrectOptions);
                }

                await _questionRepository.CreateQuestionAsync(question);

                _logger.LogInformation(
                    "User {UserId} created question {QuestionId} in challenge {ChallengeId}; previous attempts and scores were reset.",
                    userId,
                    question.QuestionId,
                    question.ChallengeId);

                TempData["SuccessMessage"] =
                    "Question created. Previous attempts and scores were reset.";

                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new
                    {
                        id = question.ChallengeId,
                        showQuestions = true
                    });
            }

            _logger.LogWarning(
                "Question creation rejected because validation failed for user {UserId} on challenge {ChallengeId}.",
                userId,
                model.Question.ChallengeId);

            PrepareAnswerFields(model);

            // Reload challenges and images if validation fails.
            if (isAdmin)
            {
                model.Challenges =
                    await _challengeRepository.GetAllChallengesAsync();
            }
            else
            {
                model.Challenges =
                    await _challengeRepository.GetChallengesByUserIdAsync(userId);
            }

            await LoadImagesAsync(userId);

            return View(model);
        }

        // Shows the Update Question form.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied question management access.",
                    userId);

                return Forbid();
            }

            var model = new QuestionFormViewModel
            {
                Question = question,

                Challenges = new List<Challenge>
                {
                    question.Challenge
                }
            };

            // Get existing answer options.
            model.Options = question.Options
                .Select(option => option.Text)
                .ToList();

            // Get all existing correct answer indexes.
            model.CorrectOptions = question.Options
                .Select((option, index) => new
                {
                    option,
                    index
                })
                .Where(x => x.option.IsCorrect)
                .Select(x => x.index)
                .ToList();

            // Get existing accepted short answers.
            model.AcceptedAnswers = question.AcceptedAnswers
                .Select(answer => answer.Text)
                .ToList();

            PrepareAnswerFields(model);

            await LoadImagesAsync(userId);

            return View(model);
        }

        // Updates an existing question.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            QuestionFormViewModel model,
            IFormFile? imageUpload)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            if (model.Question == null)
            {
                _logger.LogWarning(
                    "Question update rejected because the question data was missing.");

                return BadRequest();
            }

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(
                        model.Question.QuestionId);

            if (question == null)
            {
                return NotFound();
            }

            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied question management access.",
                    userId);

                return Forbid();
            }

            // Navigation property is not submitted by the form.
            ModelState.Remove("Question.Challenge");

            // ImageUrl may be empty.
            ModelState.Remove("Question.ImageUrl");

            if (imageUpload != null &&
                !string.IsNullOrWhiteSpace(model.Question.ImageUrl))
            {
                ModelState.AddModelError(
                    "Question.ImageUrl",
                    "Choose either an existing image or an upload.");
            }

            // Admins may keep an image belonging to another user,
            // but cannot select a different image belonging to that user.
            bool keepingExistingImage =
                model.Question.ImageUrl == question.ImageUrl;

            if (!keepingExistingImage &&
                !await IsAllowedImageAsync(
                    model.Question.ImageUrl, userId))
            {
                ModelState.AddModelError(
                    "Question.ImageUrl",
                    "This image is not available to you.");
            }

            ValidateAnswers(model);

            if (ModelState.IsValid && imageUpload != null)
            {
                string? url = await SaveImageAsync(imageUpload, userId);

                if (url != null)
                {
                    model.Question.ImageUrl = url;
                }
            }

            if (ModelState.IsValid)
            {
                // The challenge content is changing,
                // so previous attempt history is reset.
                await _attemptRepository.DeleteChallengeHistoryAsync(
                    question.ChallengeId);

                question.Title = model.Question.Title;
                question.Description = model.Question.Description;
                question.Explanation = model.Question.Explanation;
                question.Points = model.Question.Points;
                question.ImageUrl =
                    model.Question.ImageUrl ?? string.Empty;
                question.QuestionType = model.Question.QuestionType;

                // ChallengeId is deliberately not changed.

                // Remove answers from the previous question type.
                question.Options.Clear();
                question.AcceptedAnswers.Clear();

                if (question.QuestionType == "ShortAnswer")
                {
                    AddAcceptedAnswers(
                        question,
                        model.AcceptedAnswers);
                }
                else
                {
                    AddAnswerOptions(
                        question,
                        model.Options,
                        model.CorrectOptions);
                }

                await _questionRepository.UpdateQuestionAsync(question);

                _logger.LogInformation(
                    "User {UserId} updated question {QuestionId} in challenge {ChallengeId}; previous attempts and scores were reset.",
                    userId,
                    question.QuestionId,
                    question.ChallengeId);

                TempData["SuccessMessage"] =
                    "Question updated. Previous attempts and scores were reset.";

                return RedirectToAction(
                    "Details",
                    "Challenge",
                    new
                    {
                        id = question.ChallengeId,
                        showQuestions = true
                    });
            }

            _logger.LogWarning(
                "Question update rejected because validation failed for user {UserId} on question {QuestionId}.",
                userId,
                question.QuestionId);

            PrepareAnswerFields(model);

            model.Question.ChallengeId = question.ChallengeId;

            model.Challenges = new List<Challenge>
            {
                question.Challenge
            };

            await LoadImagesAsync(userId);

            return View(model);
        }

        // Shows the Delete Question confirmation page.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied question management access.",
                    userId);

                return Forbid();
            }

            return View(question);
        }

        // Deletes the question.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);
            bool isAdmin = User.IsInRole("Admin");

            var question =
                await _questionRepository
                    .GetQuestionWithOptionsAndChallengeAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            if (!isAdmin &&
                question.Challenge.CreatedByUserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} was denied question management access.",
                    userId);

                return Forbid();
            }

            int challengeId = question.ChallengeId;

            // A published challenge must always contain
            // at least one question.
            bool shouldUnpublish =
                question.Challenge.IsPublished &&
                question.Challenge.Questions.Count == 1;

            await _attemptRepository.DeleteChallengeHistoryAsync(
                challengeId);

            if (shouldUnpublish)
            {
                question.Challenge.IsPublished = false;
            }

            await _questionRepository.DeleteQuestionAsync(question);

            _logger.LogInformation(
                "User {UserId} deleted question {QuestionId} in challenge {ChallengeId}; previous attempts and scores were reset.",
                userId,
                id,
                challengeId);

            if (shouldUnpublish)
            {
                _logger.LogInformation(
                    "Challenge {ChallengeId} was automatically unpublished after its last question was deleted.",
                    challengeId);

                TempData["SuccessMessage"] =
                    "Question deleted. The challenge was automatically unpublished because it has no questions left. Previous attempts and scores were reset.";
            }
            else
            {
                TempData["SuccessMessage"] =
                    "Question deleted. Previous attempts and scores were reset.";
            }

            return RedirectToAction(
                "Details",
                "Challenge",
                new
                {
                    id = challengeId,
                    showQuestions = true
                });
        }

        // Validates answers based on question type.
        private void ValidateAnswers(QuestionFormViewModel model)
        {
            if (model.Question.QuestionType != "SingleChoice" &&
                model.Question.QuestionType != "MultipleChoice" &&
                model.Question.QuestionType != "ShortAnswer")
            {
                ModelState.AddModelError(
                    "Question.QuestionType",
                    "Please select a valid question type.");

                return;
            }

            if (model.Question.QuestionType == "ShortAnswer")
            {
                ValidateAcceptedAnswers(model);
                return;
            }

            ValidateAnswerOptions(model);
        }

        // Validates Single Choice and Multiple Choice answers.
        private void ValidateAnswerOptions(QuestionFormViewModel model)
        {
            if (model.Options == null)
            {
                ModelState.AddModelError(
                    "Options",
                    "Enter between 2 and 4 answer options.");

                return;
            }

            var filledOptions = model.Options
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option.Trim())
                .ToList();

            if (model.Options.Count > 4 ||
                filledOptions.Count < 2)
            {
                ModelState.AddModelError(
                    "Options",
                    "Enter between 2 and 4 answer options.");
            }

            if (filledOptions.Any(option => option.Length > 200))
            {
                ModelState.AddModelError(
                    "Options",
                    "Each answer option must be at most 200 characters.");
            }

            if (filledOptions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != filledOptions.Count)
            {
                ModelState.AddModelError(
                    "Options",
                    "Answer options must be different.");
            }

            model.CorrectOptions ??= new List<int>();

            var correctOptions = model.CorrectOptions
                .Distinct()
                .ToList();

            if (correctOptions.Count == 0)
            {
                ModelState.AddModelError(
                    "CorrectOptions",
                    "Select at least one correct answer.");

                return;
            }

            bool containsInvalidCorrectOption =
                correctOptions.Any(index =>
                    index < 0 ||
                    index >= model.Options.Count ||
                    index > 3 ||
                    string.IsNullOrWhiteSpace(model.Options[index]));

            if (containsInvalidCorrectOption)
            {
                ModelState.AddModelError(
                    "CorrectOptions",
                    "Correct answers must use filled answer options.");
            }

            if (model.Question.QuestionType == "SingleChoice" &&
                correctOptions.Count != 1)
            {
                ModelState.AddModelError(
                    "CorrectOptions",
                    "Single choice questions must have exactly one correct answer.");
            }
        }

        // Validates accepted answers for Short Answer questions.
        private void ValidateAcceptedAnswers(QuestionFormViewModel model)
        {
            model.AcceptedAnswers ??= new List<string>();

            var filledAnswers = model.AcceptedAnswers
                .Where(answer => !string.IsNullOrWhiteSpace(answer))
                .Select(answer => answer.Trim())
                .ToList();

            if (model.AcceptedAnswers.Count > 4 ||
                filledAnswers.Count == 0)
            {
                ModelState.AddModelError(
                    "AcceptedAnswers",
                    "Enter between 1 and 4 accepted answers.");
            }

            if (filledAnswers.Any(answer => answer.Length > 200))
            {
                ModelState.AddModelError(
                    "AcceptedAnswers",
                    "Each accepted answer must be at most 200 characters.");
            }

            if (filledAnswers
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != filledAnswers.Count)
            {
                ModelState.AddModelError(
                    "AcceptedAnswers",
                    "Accepted answers must be different.");
            }
        }

        // Adds answer options for choice questions.
        private static void AddAnswerOptions(
            Question question,
            List<string> options,
            List<int> correctOptions)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(options[i]))
                {
                    continue;
                }

                var option = new QuestionOption
                {
                    Text = options[i].Trim(),
                    IsCorrect = correctOptions.Contains(i),
                    Question = question
                };

                question.Options.Add(option);
            }
        }

        // Adds accepted answers for Short Answer questions.
        private static void AddAcceptedAnswers(
            Question question,
            List<string> acceptedAnswers)
        {
            foreach (string answer in acceptedAnswers)
            {
                if (string.IsNullOrWhiteSpace(answer))
                {
                    continue;
                }

                var acceptedAnswer = new QuestionAcceptedAnswer
                {
                    Text = answer.Trim(),
                    Question = question
                };

                question.AcceptedAnswers.Add(acceptedAnswer);
            }
        }

        // Razor renders four fields even after
        // a malformed collection is rejected.
        private static void PrepareAnswerFields(
            QuestionFormViewModel model)
        {
            model.Options =
                (model.Options ?? new List<string>())
                    .Take(4)
                    .ToList();

            while (model.Options.Count < 4)
            {
                model.Options.Add("");
            }

            model.CorrectOptions ??= new List<int>();

            model.AcceptedAnswers =
                (model.AcceptedAnswers ?? new List<string>())
                    .Take(4)
                    .ToList();

            while (model.AcceptedAnswers.Count < 4)
            {
                model.AcceptedAnswers.Add("");
            }
        }
    }
}
