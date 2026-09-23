using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public QuestionController(ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        // Shows all questions
        public async Task<IActionResult> Table()
        {
            List<Question> questions =
                await _challengeDbContext.Questions.ToListAsync();

            return View(questions);
        }

        // Shows the Create Question form
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateQuestionViewModel
            {
                Challenges = _challengeDbContext.Challenges.ToList()
            };

            return View(model);
        }

        // Creates a new question with answer options
        [HttpPost]
        public IActionResult Create(CreateQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var question = model.Question;

                for (int i = 0; i < model.Options.Count; i++)
                {
                    var option = new QuestionOption
                    {
                        Text = model.Options[i],
                        IsCorrect = i == model.CorrectOption,
                        Question = question
                    };

                    question.Options.Add(option);
                }

                _challengeDbContext.Questions.Add(question);
                _challengeDbContext.SaveChanges();

                return RedirectToAction(nameof(Table));
            }

            model.Challenges = _challengeDbContext.Challenges.ToList();

            return View(model);
        }

        // Shows the Update Question form
        [HttpGet]
        public IActionResult Update(int id)
        {
            var question = _challengeDbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            var model = new CreateQuestionViewModel
            {
                Question = question,
                Challenges = _challengeDbContext.Challenges.ToList()
            };

            // Get existing answer options
            model.Options = question.Options
                .Select(option => option.Text)
                .ToList();

            // Make sure there are always 4 option fields
            while (model.Options.Count < 4)
            {
                model.Options.Add("");
            }

            // Find which option is correct
            var correctOption = question.Options
                .Select((option, index) => new { option, index })
                .FirstOrDefault(x => x.option.IsCorrect);

            if (correctOption != null)
            {
                model.CorrectOption = correctOption.index;
            }

            return View(model);
        }

        // Updates an existing question
        [HttpPost]
        public IActionResult Update(CreateQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var question = _challengeDbContext.Questions
                    .Include(q => q.Options)
                    .FirstOrDefault(q =>
                        q.QuestionId == model.Question.QuestionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Update question information
                question.Title = model.Question.Title;
                question.Description = model.Question.Description;
                question.Points = model.Question.Points;
                question.ImageUrl = model.Question.ImageUrl;
                question.ChallengeId = model.Question.ChallengeId;

                // Update answer options
                for (int i = 0;
                     i < question.Options.Count && i < model.Options.Count;
                     i++)
                {
                    question.Options[i].Text = model.Options[i];

                    question.Options[i].IsCorrect =
                        i == model.CorrectOption;
                }

                _challengeDbContext.SaveChanges();

                return RedirectToAction(nameof(Table));
            }

            // Reload challenges if validation fails
            model.Challenges = _challengeDbContext.Challenges.ToList();

            // Make sure there are always 4 option fields
            while (model.Options.Count < 4)
            {
                model.Options.Add("");
            }

            return View(model);
        }

        // Shows the Delete Question confirmation page
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var question = _challengeDbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // Deletes the question
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var question = _challengeDbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            // Delete the answer options belonging to the question
            _challengeDbContext.QuestionOptions.RemoveRange(question.Options);

            // Delete the question
            _challengeDbContext.Questions.Remove(question);

            _challengeDbContext.SaveChanges();

            return RedirectToAction(nameof(Table));
        }

    }
}