using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Models
{
    public static class DBInit
    {
        public static void Seed(IApplicationBuilder app)
        {
            using var serviceScope =
                app.ApplicationServices.CreateScope();

            ChallengeDbContext context =
                serviceScope.ServiceProvider
                    .GetRequiredService<ChallengeDbContext>();

            // Delete the existing database and create it again.
            // Useful while testing with dummy data.
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Add Challenges
            if (!context.Challenges.Any())
            {
                var challenges = new List<Challenge>
                {
                    new Challenge
                    {
                        Title = "HTML Quiz",
                        Description = "Test your knowledge of HTML.",
                        MaxPoints = 20,
                        ImageUrl = "/images/Question_mark_code_10.png"
                    },

                    new Challenge
                    {
                        Title = "Programming Challenge",
                        Description = "Test your programming knowledge.",
                        MaxPoints = 10,
                        ImageUrl = "/images/Question_mark_code_7.png"
                    },

                    new Challenge
                    {
                        Title = "Web Development",
                        Description = "Answer questions about web development.",
                        MaxPoints = 20,
                        ImageUrl = "/images/Question_mark_code_7.png"
                    }
                };

                context.AddRange(challenges);
                context.SaveChanges();
            }

            // Add Questions
            if (!context.Questions.Any())
            {
                var questions = new List<Question>
                {
                    new Question
                    {
                        Title = "What does HTML stand for?",
                        Description = "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 1
                    },

                    new Question
                    {
                        Title = "Which HTML tag is used for a paragraph?",
                        Description = "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 1
                    },

                    new Question
                    {
                        Title = "Which language is mainly used to style web pages?",
                        Description = "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 3
                    },

                    new Question
                    {
                        Title = "What does CSS stand for?",
                        Description = "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 3
                    },

                    new Question
                    {
                        Title = "Which keyword is used to create a variable in C#?",
                        Description = "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 2
                    }
                };

                context.AddRange(questions);
                context.SaveChanges();
            }

            // Add answer options
            if (!context.QuestionOptions.Any())
            {
                var questionOptions = new List<QuestionOption>
                {
                    // Question 1
                    new QuestionOption
                    {
                        Text = "Hyper Text Markup Language",
                        IsCorrect = true,
                        QuestionId = 1
                    },

                    new QuestionOption
                    {
                        Text = "High Tech Modern Language",
                        IsCorrect = false,
                        QuestionId = 1
                    },

                    new QuestionOption
                    {
                        Text = "Hyper Transfer Markup Language",
                        IsCorrect = false,
                        QuestionId = 1
                    },

                    // Question 2
                    new QuestionOption
                    {
                        Text = "<p>",
                        IsCorrect = true,
                        QuestionId = 2
                    },

                    new QuestionOption
                    {
                        Text = "<div>",
                        IsCorrect = false,
                        QuestionId = 2
                    },

                    new QuestionOption
                    {
                        Text = "<h1>",
                        IsCorrect = false,
                        QuestionId = 2
                    },

                    // Question 3
                    new QuestionOption
                    {
                        Text = "CSS",
                        IsCorrect = true,
                        QuestionId = 3
                    },

                    new QuestionOption
                    {
                        Text = "HTML",
                        IsCorrect = false,
                        QuestionId = 3
                    },

                    new QuestionOption
                    {
                        Text = "C#",
                        IsCorrect = false,
                        QuestionId = 3
                    },

                    // Question 4
                    new QuestionOption
                    {
                        Text = "Cascading Style Sheets",
                        IsCorrect = true,
                        QuestionId = 4
                    },

                    new QuestionOption
                    {
                        Text = "Computer Style System",
                        IsCorrect = false,
                        QuestionId = 4
                    },

                    new QuestionOption
                    {
                        Text = "Creative Style Sheets",
                        IsCorrect = false,
                        QuestionId = 4
                    },

                    // Question 5
                    new QuestionOption
                    {
                        Text = "var",
                        IsCorrect = true,
                        QuestionId = 5
                    },

                    new QuestionOption
                    {
                        Text = "variable",
                        IsCorrect = false,
                        QuestionId = 5
                    },

                    new QuestionOption
                    {
                        Text = "let",
                        IsCorrect = false,
                        QuestionId = 5
                    }
                };

                context.AddRange(questionOptions);
                context.SaveChanges();
            }

            // Add Users
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "Alice",
                        Email = "alice@example.com"
                    },

                    new User
                    {
                        Username = "Bob",
                        Email = "bob@example.com"
                    },

                    new User
                    {
                        Username = "Charlie",
                        Email = "charlie@example.com"
                    }
                };

                context.AddRange(users);
                context.SaveChanges();
            }

            // Add UserChallenges
            if (!context.UserChallenges.Any())
            {
                var userChallenges = new List<UserChallenge>
                {
                    new UserChallenge
                    {
                        UserId = 1,
                        ChallengeId = 1
                    },

                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 1
                    },

                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 2
                    }
                };

                context.AddRange(userChallenges);
                context.SaveChanges();
            }

            // Add ChallengeAttempts
            if (!context.ChallengeAttempts.Any())
            {
                var challengeAttempts =
                    new List<ChallengeAttempt>
                {
                    // Alice - first attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 1,
                        Score = 10,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddDays(-2)
                                .AddMinutes(-10),
                        CompletedAt =
                            DateTime.Now
                                .AddDays(-2)
                    },

                    // Alice - best attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 1,
                        Score = 20,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddDays(-1)
                                .AddMinutes(-8),
                        CompletedAt =
                            DateTime.Now
                                .AddDays(-1)
                    },

                    // Bob - same score as Alice,
                    // but slower
                    new ChallengeAttempt
                    {
                        UserChallengeId = 2,
                        Score = 20,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddDays(-1)
                                .AddMinutes(-12),
                        CompletedAt =
                            DateTime.Now
                                .AddDays(-1)
                    },

                    // Charlie - Programming Challenge
                    new ChallengeAttempt
                    {
                        UserChallengeId = 3,
                        Score = 10,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddMinutes(-15),
                        CompletedAt =
                            DateTime.Now
                    }
                };

                context.AddRange(challengeAttempts);
                context.SaveChanges();
            }
        }
    }
}