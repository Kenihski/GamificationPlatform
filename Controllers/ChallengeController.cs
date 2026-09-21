using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Table()
        {
            List<Challenge> challenges = _challengeDbContext.Challenges.ToList();
            var challengesViewModel = new ChallengesViewModel(challenges, "Table");
            return View(challengesViewModel);
        }

        public IActionResult Grid()
        {
            List<Challenge> challenges = _challengeDbContext.Challenges.ToList();
            var challengesViewModel = new ChallengesViewModel(challenges, "Grid");
            return View(challengesViewModel);
        }

        public IActionResult Details(int id)
        {
            List<Challenge> challenges = _challengeDbContext.Challenges.ToList();
            var challenge = challenges.FirstOrDefault(i => i.ChallengeId ==id);
            if (challenge == null)
                return NotFound();
            return View(challenge);
        }
      
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Add(challenge);
                _challengeDbContext.SaveChanges();
                return RedirectToAction(nameof(Table));
            }
            return View(challenge);
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var challenge = _challengeDbContext.Challenges.Find(id);
            if (challenge == null)
            {
                return NotFound();
            }
            return View(challenge);
        }

        [HttpPost]
        public IActionResult Update(Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                _challengeDbContext.Challenges.Update(challenge);
                _challengeDbContext.SaveChanges();
                return RedirectToAction(nameof(Table));
            }
            return View(challenge);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var challenge = _challengeDbContext.Challenges.Find(id);
            if (challenge == null)
            {
                return NotFound();
            }
            return View(challenge);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var challenge = _challengeDbContext.Challenges.Find(id);
            if (challenge == null)
            {
                return NotFound();
            }
            _challengeDbContext.Challenges.Remove(challenge);
            _challengeDbContext.SaveChanges();
            return RedirectToAction(nameof(Table));
        }

       // public IActionResult Table()
       // {
       //     var challenges = GetChallenges();
       //    ViewBag.CurrentViewName = "Table";
       //     return View(challenges);
       // }

        // public IActionResult Grid()
        // {
        // var challenges = GetChallenges();
        //   ViewBag.CurrentViewName = "Grid";
        //   return View(challenges);
        // }

        private List<Challenge> GetChallenges()
        {
            var challenges = new List<Challenge>();
            var challenge1 = new Challenge
            {
                ChallengeId = 1,
                Title = "Solve a Coding Puzzle",
                Description = "Complete a small algorithmic challenge.",
                MaxPoints = 50,
                ImageUrl = "/images/Question_mark_code_10.png"
            };

            var challenge2 = new Challenge
            {
                ChallengeId = 2,
                Title = "Write a Reflection",
                Description = "Write a short reflection about today's lecture.",
                MaxPoints = 20,
                ImageUrl = "/images/Question_mark_code_2.png"
            };

            var challenge3 = new Challenge
            {
                ChallengeId = 3,
                Title = "Complete a Quiz",
                Description = "Complete a quiz about the topics covered in class.",
                MaxPoints = 30,
                ImageUrl = "/images/Question_mark_code_3.png"
            };

            var challenge4 = new Challenge
            {
                ChallengeId = 4,
                Title = "Build a Website",
                Description = "Create a simple website using HTML and CSS.",
                MaxPoints = 100,
                ImageUrl = "/images/Question_mark_code_4.png"
            };

            var challenge5 = new Challenge
            {
                ChallengeId = 5,
                Title = "Debug the Code",
                Description = "Find and fix the errors in the provided code.",
                MaxPoints = 40,
                ImageUrl = "/images/Question_mark_code_5.png"
            };

            var challenge6 = new Challenge
            {
                ChallengeId = 6,
                Title = "Team Challenge",
                Description = "Work together with your classmates to solve a problem.",
                MaxPoints = 60,
                ImageUrl = "/images/Question_mark_code_6.png"
            };

            var challenge7 = new Challenge
            {
                ChallengeId = 7,
                Title = "Learn Something New",
                Description = "Learn about a new programming concept and explain it.",
                MaxPoints = 25,
                ImageUrl = "/images/Question_mark_code_7.png"
            };

            var challenge8 = new Challenge
            {
                ChallengeId = 8,
                Title = "Final Project",
                Description = "Complete a small project using what you have learned.",
                MaxPoints = 150,
                ImageUrl = "/images/Question_mark_code_9.png"
            };

            challenges.Add(challenge1);
            challenges.Add(challenge2);
            challenges.Add(challenge3);
            challenges.Add(challenge4);
            challenges.Add(challenge5);
            challenges.Add(challenge6);
            challenges.Add(challenge7);
            challenges.Add(challenge8);

            return challenges;
        }
    }
}