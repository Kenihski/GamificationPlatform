using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class ChallengeController : Controller
    {

        public IActionResult Table()
        {
            var challenges = GetChallenges();
            var challengesViewModel = new ChallengesViewModel(challenges, "Table");
            return View(challengesViewModel);
        }

        public IActionResult Grid()
        {
            var challenges = GetChallenges();
            var challengesViewModel = new ChallengesViewModel(challenges, "Grid");

            return View(challengesViewModel);
        }

        public IActionResult Details(int id)
        {
            var challenges = GetChallenges();
            var challenge = challenges.FirstOrDefault(i => i.ChallengeId ==id);
            if (challenge == null)
                return NotFound();
            return View(challenge);
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
                ImageUrl = "/images/Challenge.png"
            };

            var challenge2 = new Challenge
            {
                ChallengeId = 2,
                Title = "Write a Reflection",
                Description = "Write a short reflection about today's lecture.",
                MaxPoints = 20,
                ImageUrl = "/images/Question_mark.png"
            };

            var challenge3 = new Challenge
            {
                ChallengeId = 3,
                Title = "Complete a Quiz",
                Description = "Complete a quiz about the topics covered in class.",
                MaxPoints = 30,
                ImageUrl = "/images/Retro_Challenge.png"
            };

            var challenge4 = new Challenge
            {
                ChallengeId = 4,
                Title = "Build a Website",
                Description = "Create a simple website using HTML and CSS.",
                MaxPoints = 100,
                ImageUrl = "/images/Retro_question_mark.png"
            };

            var challenge5 = new Challenge
            {
                ChallengeId = 5,
                Title = "Debug the Code",
                Description = "Find and fix the errors in the provided code.",
                MaxPoints = 40,
                ImageUrl = "/images/Challenge.png"
            };

            var challenge6 = new Challenge
            {
                ChallengeId = 6,
                Title = "Team Challenge",
                Description = "Work together with your classmates to solve a problem.",
                MaxPoints = 60,
                ImageUrl = "/images/Question_mark.png"
            };

            var challenge7 = new Challenge
            {
                ChallengeId = 7,
                Title = "Learn Something New",
                Description = "Learn about a new programming concept and explain it.",
                MaxPoints = 25,
                ImageUrl = "/images/Retro_Challenge.png"
            };

            var challenge8 = new Challenge
            {
                ChallengeId = 8,
                Title = "Final Project",
                Description = "Complete a small project using what you have learned.",
                MaxPoints = 150,
                ImageUrl = "/images/Retro_question_mark.png"
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