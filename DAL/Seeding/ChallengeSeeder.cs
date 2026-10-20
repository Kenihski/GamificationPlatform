
using Microsoft.AspNetCore.Identity;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public static class ChallengeSeeder
    {
        public static void Seed(ChallengeDbContext context)
        {
            // Challenges
            // --------------------------------
            var challenges = new List<Challenge>
            {
                new Challenge
                {
                    Title = "HTML Quiz",
                    Description =
                        "Test your knowledge of HTML.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/HTML.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "Programming Challenge",
                    Description =
                        "Test your basic programming knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Programming.png",
                    TimeLimitMinutes = 20,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "Web Development",
                    Description =
                        "Answer questions about web development.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Web development.png",
                    TimeLimitMinutes = 30,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "C# Quiz",
                    Description =
                        "Test your knowledge of C#.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/CSharp.png",
                    TimeLimitMinutes = 20,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "Database Quiz",
                    Description =
                        "Test your database knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Databases.png",
                    TimeLimitMinutes = null,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "CSS Challenge",
                    Description =
                        "Test your knowledge of CSS and styling.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/CSS.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 2
                },
                new Challenge
                {
                    Title = "ASP.NET Core Quiz",
                    Description =
                        "Test your ASP.NET Core knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/ASP.NET.png",
                    TimeLimitMinutes = 60,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 3
                },
                // Charlie
                // Community challenge.
                new Challenge
                {
                    Title = "JavaScript Quiz",
                    Description =
                        "Test your JavaScript knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Javascript.png",
                    TimeLimitMinutes = 30,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 4
                },
                new Challenge
                {
                    Title = "Python Quiz",
                    Description =
                        "Test your basic Python knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Python.png",
                    TimeLimitMinutes = 20,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 3
                },
                new Challenge
                {
                    Title = "SQL Challenge",
                    Description =
                        "Test your knowledge of SQL queries.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/SQL.png",
                    TimeLimitMinutes = 20,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 5
                },
                new Challenge
                {
                    Title = "Git Challenge",
                    Description =
                        "Test your knowledge of Git and version control.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/GitHub.png",
                    TimeLimitMinutes = 20,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 6
                },
                new Challenge
                {
                    Title = "Cybersecurity Quiz",
                    Description =
                        "Test your basic cybersecurity knowledge.",
                    MaxPoints = 0,
                    ImageUrl =
                        "/images/Cybersecurity.png",
                    TimeLimitMinutes = 30,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 2
                }
            };

            context.Challenges.AddRange(challenges);
            context.SaveChanges();
        }
    }
}
