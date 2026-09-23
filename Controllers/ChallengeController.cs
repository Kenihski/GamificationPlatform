using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                await _challengeDbContext.Challenges.ToListAsync();

            var challengesViewModel =
                new ChallengesViewModel(challenges, "Table");

            return View(challengesViewModel);
        }

        // Shows challenges in a grid
        public async Task<IActionResult> Grid()
        {
            List<Challenge> challenges =
                await _challengeDbContext.Challenges.ToListAsync();

            var challengesViewModel =
                new ChallengesViewModel(challenges, "Grid");

            return View(challengesViewModel);
        }

        // Shows information about a challenge
        public async Task<IActionResult> Details(int id)
        {
            var challenge = await _challengeDbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeId == id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // Shows the quiz and its questions
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

            return View(challenge);
        }

        // Checks the answers and calculates the score
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            int challengeId,
            Dictionary<int, int> answers)
        {
            var challenge = await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c => c.ChallengeId == challengeId);

            if (challenge == null)
            {
                return NotFound();
            }

            int score = 0;

            // Calculate the score
            foreach (var question in challenge.Questions)
            {
                if (answers.TryGetValue(
                    question.QuestionId,
                    out int selectedOptionId))
                {
                    var selectedOption = question.Options
                        .FirstOrDefault(o =>
                            o.QuestionOptionId == selectedOptionId);

                    if (selectedOption != null && selectedOption.IsCorrect)
                    {
                        score += question.Points;
                    }
                }
            }

            ViewBag.Score = score;
            ViewBag.MaxScore = challenge.Questions.Sum(q => q.Points);

            return View("Result", challenge);
        }

        // Shows the Create Challenge form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Creates a new challenge
        [HttpPost]
        public async Task<IActionResult> Create(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Add(challenge);

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
                await _challengeDbContext.Challenges.FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // Updates an existing challenge
        [HttpPost]
        public async Task<IActionResult> Update(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Update(challenge);

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
                await _challengeDbContext.Challenges.FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // Deletes a challenge
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var challenge =
                await _challengeDbContext.Challenges.FindAsync(id);

            if (challenge == null)
            {
                return NotFound();
            }

            _challengeDbContext.Challenges.Remove(challenge);

            await _challengeDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Table));
        }
    }
}