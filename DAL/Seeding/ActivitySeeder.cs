
using Microsoft.AspNetCore.Identity;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public static class ActivitySeeder
    {
        public static async Task SeedAsync(
            ChallengeDbContext context,
            UserManager<User> userManager)
        {
            // Users with IDs 7–12 are created by UserSeeder.
            var extraUserIds = new[] { 7, 8, 9, 10, 11, 12 };

            var extraChallenges = new List<Challenge>
            {
                new Challenge
                {
                    Title = "Accessibility Basics",
                    Description = "Test basic web accessibility knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Accessibility.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 2
                },
                new Challenge
                {
                    Title = "Linux Basics",
                    Description = "Test basic Linux knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Linux.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 2
                },
                new Challenge
                {
                    Title = "Network Basics",
                    Description = "Test basic networking knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Network.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 2
                },
                new Challenge
                {
                    Title = "Cloud Basics",
                    Description = "Test basic cloud knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Cloud.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 3
                },
                new Challenge
                {
                    Title = "API Basics",
                    Description = "Test basic API knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/API.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 3
                },
                new Challenge
                {
                    Title = "Testing Basics",
                    Description = "Test basic software testing knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Testing.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 3
                },
                new Challenge
                {
                    Title = "Security Basics",
                    Description = "Test basic application security knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Security.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                },
                new Challenge
                {
                    Title = "Software Architecture",
                    Description = "Test basic software architecture knowledge.",
                    MaxPoints = 10,
                    ImageUrl = "/images/Software architecture.png",
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = true,
                    CreatedByUserId = 1
                }
            };

            context.Challenges.AddRange(extraChallenges);
            context.SaveChanges();

            var additionalQuestions = new (string Title, string Correct, string Incorrect1, string Incorrect2)[]
            {
                ("What is the purpose of alternative text on an image?", "To describe the image to assistive technologies", "To increase the image resolution", "To change the image file format"),
                ("Which Linux command displays the current directory?", "pwd", "mkdir", "touch"),
                ("Which protocol is used to translate domain names into IP addresses?", "DNS", "FTP", "SSH"),
                ("What is a key benefit of cloud computing?", "Resources can be scaled according to demand", "Applications no longer need security", "Internet connectivity is never required"),
                ("Which HTTP method is normally used to retrieve a resource?", "GET", "DELETE", "PATCH"),
                ("What is the main purpose of a unit test?", "To verify a small part of an application", "To replace all integration tests", "To deploy the application"),
                ("What is SQL injection?", "Inserting malicious SQL through untrusted input", "Encrypting database backups", "Adding an index to a database table"),
                ("What is the main purpose of dependency injection?", "To provide dependencies from outside a class", "To store passwords in source code", "To remove the need for interfaces")
            };

            for (int i = 0; i < additionalQuestions.Length; i++)
            {
                var item = additionalQuestions[i];

                var question = new Question
                {
                    Title = item.Title,
                    Description = "Choose the correct answer.",
                    Points = 10,
                    QuestionType = "SingleChoice",
                    ChallengeId = extraChallenges[i].ChallengeId
                };

                context.Questions.Add(question);
                context.SaveChanges();

                context.QuestionOptions.AddRange(
                    new QuestionOption
                    {
                        Text = item.Correct,
                        IsCorrect = true,
                        QuestionId = question.QuestionId
                    },
                    new QuestionOption
                    {
                        Text = item.Incorrect1,
                        IsCorrect = false,
                        QuestionId = question.QuestionId
                    },
                    new QuestionOption
                    {
                        Text = item.Incorrect2,
                        IsCorrect = false,
                        QuestionId = question.QuestionId
                    });
            }

            context.SaveChanges();

            // Keep demo activity consistent with the question bank.
            var now = DateTime.Now;

            // Alice completes challenges created by other users.
            int dayOffset = 30;

            foreach (var challenge in context.Challenges
                .Where(c => c.CreatedByUserId != 2)
                .OrderBy(c => c.ChallengeId)
                .ToList())
            {
                bool alreadyCompleted = context.ChallengeAttempts
                    .Any(a =>
                        a.Completed &&
                        a.UserChallenge.UserId == 2 &&
                        a.UserChallenge.ChallengeId == challenge.ChallengeId);

                if (!alreadyCompleted)
                {
                    int count = context.Questions.Count(
                        q => q.ChallengeId == challenge.ChallengeId);

                    AttemptSeeder.AddAttempt(
                        context,
                        2,
                        challenge.ChallengeId,
                        count,
                        now.AddDays(-dayOffset--));
                }
            }

            // Repeat HTML Quiz to demonstrate improvement.
            AttemptSeeder.AddAttempt(
                context, 2, 1, 2, now.AddHours(-8));

            AttemptSeeder.AddAttempt(
                context,
                2,
                1,
                context.Questions.Count(q => q.ChallengeId == 1),
                now.AddHours(-4),
                4);

            // Ten participants on Alice's Accessibility challenge.
            int participantNumber = 0;

            foreach (int userId in new[]
            {
                3, 4, 5, 6, 7, 8, 9, 10, 11, 12
            })
            {
                AttemptSeeder.AddAttempt(
                    context,
                    userId,
                    extraChallenges[0].ChallengeId,
                    participantNumber < 3 ? 1 : 0,
                    now.AddDays(-2).AddMinutes(-participantNumber),
                    8 + participantNumber);

                participantNumber++;
            }

            // Bob: additional challenges and repeat attempts.
            foreach (int challengeId in new[]
            {
                2, 5, 8, 9, 10, 19, 20
            })
            {
                int count = context.Questions.Count(
                    q => q.ChallengeId == challengeId);

                AttemptSeeder.AddAttempt(
                    context,
                    3,
                    challengeId,
                    count,
                    now.AddDays(-12 - challengeId),
                    7);
            }

            for (int i = 0; i < 4; i++)
            {
                AttemptSeeder.AddAttempt(
                    context,
                    3,
                    3,
                    3,
                    now.AddDays(-1).AddHours(-(i + 1)));
            }

            // David: complete more challenges to earn additional achievements.
            foreach (int challengeId in new[]
            {
                6, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20
            })
            {
                int count = context.Questions.Count(
                    q => q.ChallengeId == challengeId);

                AttemptSeeder.AddAttempt(
                    context,
                    6,
                    challengeId,
                    count,
                    now.AddDays(-18 - challengeId));
            }

            AttemptSeeder.AddAttempt(
                context,
                4,
                extraChallenges[1].ChallengeId,
                1,
                now.AddDays(-6),
                6);

            for (int i = 0; i < extraUserIds.Length; i++)
            {
                int challengeId =
                    extraChallenges[1 + (i % 3)].ChallengeId;

                AttemptSeeder.AddAttempt(
                    context,
                    extraUserIds[i],
                    challengeId,
                    i % 2 == 0 ? 1 : 0,
                    now.AddDays(-(3 + i)),
                    6);
            }

            var furtherQuizzes = new (
                string Title,
                string Description,
                string Image,
                string Question,
                string Correct,
                string Incorrect1,
                string Incorrect2)[]
            {
                (
                    "Java Fundamentals",
                    "Explore Java syntax and object-oriented programming.",
                    "/images/Java.png",
                    "Which keyword is used to inherit a class in Java?",
                    "extends",
                    "implements",
                    "inherits"
                ),
                (
                    "Python Data Structures",
                    "Practice lists, dictionaries and other Python collections.",
                    "/images/Python.png",
                    "Which Python collection stores key-value pairs?",
                    "dict",
                    "list",
                    "tuple"
                ),
                (
                    "SQL Joins",
                    "Learn how to combine data from related database tables.",
                    "/images/SQL.png",
                    "Which SQL join returns only matching rows from both tables?",
                    "INNER JOIN",
                    "LEFT JOIN",
                    "FULL OUTER JOIN"
                ),
                (
                    "Git Branching",
                    "Practice working with branches in version control.",
                    "/images/GitHub.png",
                    "Which Git command creates a new branch without switching to it?",
                    "git branch feature",
                    "git merge feature",
                    "git pull feature"
                ),
                (
                    "HTTP and REST",
                    "Explore HTTP status codes and REST API conventions.",
                    "/images/API.png",
                    "Which HTTP status code means a resource was not found?",
                    "404",
                    "200",
                    "201"
                )
            };

            foreach (var item in furtherQuizzes)
            {
                var challenge = new Challenge
                {
                    Title = item.Title,
                    Description = item.Description,
                    MaxPoints = 10,
                    ImageUrl = item.Image,
                    TimeLimitMinutes = 10,
                    IsPublished = true,
                    IsCore = false,
                    CreatedByUserId = 1
                };

                context.Challenges.Add(challenge);
                context.SaveChanges();

                var question = new Question
                {
                    Title = item.Question,
                    Description = "Choose the correct answer.",
                    Points = 10,
                    QuestionType = "SingleChoice",
                    ChallengeId = challenge.ChallengeId,
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption
                        {
                            Text = item.Correct,
                            IsCorrect = true
                        },
                        new QuestionOption
                        {
                            Text = item.Incorrect1,
                            IsCorrect = false
                        },
                        new QuestionOption
                        {
                            Text = item.Incorrect2,
                            IsCorrect = false
                        }
                    }
                };

                context.Questions.Add(question);
                context.SaveChanges();

                AttemptSeeder.AddAttempt(
                    context,
                    2,
                    challenge.ChallengeId,
                    1,
                    now.AddDays(
                        -(40 + Array.IndexOf(furtherQuizzes, item) + 1)));
            }

            // Bob earns Community Favorite through unique participants.
            var bobChallenge = context.Challenges
                .First(c => c.Title == "Cloud Basics");

            foreach (int userId in new[] { 2, 4, 5, 6, 7, 8, 9, 10, 11, 12 })
            {
                bool alreadyCompleted = context.ChallengeAttempts.Any(a =>
                    a.Completed &&
                    a.UserChallenge.UserId == userId &&
                    a.UserChallenge.ChallengeId == bobChallenge.ChallengeId);

                if (alreadyCompleted)
                {
                    continue;
                }

                int questionCount = context.Questions.Count(
                    q => q.ChallengeId == bobChallenge.ChallengeId);

                AttemptSeeder.AddAttempt(
                    context,
                    userId,
                    bobChallenge.ChallengeId,
                    questionCount,
                    now.AddDays(-5).AddMinutes(-userId),
                    5);
            }

            // Synchronize maximum scores with the actual questions.
            foreach (var challenge in context.Challenges.ToList())
            {
                challenge.MaxPoints = context.Questions
                    .Where(q => q.ChallengeId == challenge.ChallengeId)
                    .Sum(q => q.Points);
            }

            await context.SaveChangesAsync();
        }
    }
}
