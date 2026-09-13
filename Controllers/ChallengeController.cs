using Microsoft.AspNetCore.Mvc;
using GamificationPlatform.Models;

namespace GamificationPlatform.Controllers
{
    public class ChallengeController : Controller
    {
        public IActionResult Table()
        {
            var challenges = new List<Challenge>();

            var c1 = new Challenge
            {
                ChallengeId = 1,
                Title = "Solve a Coding Puzzle",
                Description = "Complete a small algorithmic challenge.",
                MaxPoints = 50
            };

            var c2 = new Challenge
            {
                ChallengeId = 2,
                Title = "Write a Reflection",
                Description = "Write a short reflection about today's lecture.",
                MaxPoints = 20
            };

            challenges.Add(c1);
            challenges.Add(c2);

            ViewBag.CurrentViewName = "List of Challenges";
            return View(challenges);
        }
    }
}


