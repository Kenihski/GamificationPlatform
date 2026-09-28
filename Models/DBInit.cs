using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
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

            // Add Users
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        IsAdmin = true
                    },

                    new User
                    {
                        Username = "Alice",
                        Email = "alice@example.com",
                        IsAdmin = false
                    },

                    new User
                    {
                        Username = "Bob",
                        Email = "bob@example.com",
                        IsAdmin = false
                    },

                    new User
                    {
                        Username = "Charlie",
                        Email = "charlie@example.com",
                        IsAdmin = false
                    },

                    new User
                    {
                        Username = "Emma",
                        Email = "emma@example.com",
                        IsAdmin = false
                    },

                    new User
                    {
                        Username = "David",
                        Email = "david@example.com",
                        IsAdmin = false
                    }
                };

                // Gives all dummy users a test password.
                var passwordHasher =
                    new PasswordHasher<User>();

                foreach (var user in users)
                {
                    user.PasswordHash =
                        passwordHasher.HashPassword(
                            user,
                            "Password123!");
                }

                context.AddRange(users);
                context.SaveChanges();
            }

            // Add Challenges
            if (!context.Challenges.Any())
            {
                var challenges = new List<Challenge>
                {
                    new Challenge
                    {
                        Title = "HTML Quiz",
                        Description =
                            "Test your knowledge of HTML.",
                        MaxPoints = 20,
                        ImageUrl =
                            "/images/Question_mark_code_10.png",
                        CreatedByUserId = 2
                    },

                    new Challenge
                    {
                        Title = "Programming Challenge",
                        Description =
                            "Test your programming knowledge.",
                        MaxPoints = 20,
                        ImageUrl =
                            "/images/Question_mark_code_7.png",
                        CreatedByUserId = 3
                    },

                    new Challenge
                    {
                        Title = "Web Development",
                        Description =
                            "Answer questions about web development.",
                        MaxPoints = 20,
                        ImageUrl =
                            "/images/Question_mark_code_7.png",
                        CreatedByUserId = 4
                    },

                    new Challenge
                    {
                        Title = "C# Quiz",
                        Description =
                            "Test your knowledge of C#.",
                        MaxPoints = 20,
                        ImageUrl =
                            "/images/Question_mark_code_10.png",
                        CreatedByUserId = 5
                    },

                    new Challenge
                    {
                        Title = "Database Quiz",
                        Description =
                            "Test your database knowledge.",
                        MaxPoints = 20,
                        ImageUrl =
                            "/images/Question_mark_code_10.png",
                        CreatedByUserId = 6
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
                    // HTML Quiz
                    new Question
                    {
                        Title = "What does HTML stand for?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 1
                    },

                    new Question
                    {
                        Title =
                            "Which HTML tag is used for a paragraph?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 1
                    },

                    // Programming Challenge
                    new Question
                    {
                        Title =
                            "Which keyword can be used to create a variable in C#?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 2
                    },

                    new Question
                    {
                        Title =
                            "Which symbol ends most C# statements?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 2
                    },

                    // Web Development
                    new Question
                    {
                        Title =
                            "Which language is mainly used to style web pages?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 3
                    },

                    new Question
                    {
                        Title = "What does CSS stand for?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 3
                    },

                    // C# Quiz
                    new Question
                    {
                        Title =
                            "Which type stores whole numbers in C#?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 4
                    },

                    new Question
                    {
                        Title =
                            "Which keyword creates a class in C#?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 4
                    },

                    // Database Quiz
                    new Question
                    {
                        Title =
                            "Which language is commonly used with relational databases?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 5
                    },

                    new Question
                    {
                        Title =
                            "What is a primary key used for?",
                        Description =
                            "Choose the correct answer.",
                        Points = 10,
                        ChallengeId = 5
                    }
                };

                context.AddRange(questions);
                context.SaveChanges();
            }

            // Add answer options
            if (!context.QuestionOptions.Any())
            {
                var questionOptions =
                    new List<QuestionOption>
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
                        Text = "var",
                        IsCorrect = true,
                        QuestionId = 3
                    },
                    new QuestionOption
                    {
                        Text = "variable",
                        IsCorrect = false,
                        QuestionId = 3
                    },
                    new QuestionOption
                    {
                        Text = "define",
                        IsCorrect = false,
                        QuestionId = 3
                    },

                    // Question 4
                    new QuestionOption
                    {
                        Text = ";",
                        IsCorrect = true,
                        QuestionId = 4
                    },
                    new QuestionOption
                    {
                        Text = ":",
                        IsCorrect = false,
                        QuestionId = 4
                    },
                    new QuestionOption
                    {
                        Text = "#",
                        IsCorrect = false,
                        QuestionId = 4
                    },

                    // Question 5
                    new QuestionOption
                    {
                        Text = "CSS",
                        IsCorrect = true,
                        QuestionId = 5
                    },
                    new QuestionOption
                    {
                        Text = "HTML",
                        IsCorrect = false,
                        QuestionId = 5
                    },
                    new QuestionOption
                    {
                        Text = "C#",
                        IsCorrect = false,
                        QuestionId = 5
                    },

                    // Question 6
                    new QuestionOption
                    {
                        Text = "Cascading Style Sheets",
                        IsCorrect = true,
                        QuestionId = 6
                    },
                    new QuestionOption
                    {
                        Text = "Computer Style System",
                        IsCorrect = false,
                        QuestionId = 6
                    },
                    new QuestionOption
                    {
                        Text = "Creative Style Sheets",
                        IsCorrect = false,
                        QuestionId = 6
                    },

                    // Question 7
                    new QuestionOption
                    {
                        Text = "int",
                        IsCorrect = true,
                        QuestionId = 7
                    },
                    new QuestionOption
                    {
                        Text = "string",
                        IsCorrect = false,
                        QuestionId = 7
                    },
                    new QuestionOption
                    {
                        Text = "bool",
                        IsCorrect = false,
                        QuestionId = 7
                    },

                    // Question 8
                    new QuestionOption
                    {
                        Text = "class",
                        IsCorrect = true,
                        QuestionId = 8
                    },
                    new QuestionOption
                    {
                        Text = "object",
                        IsCorrect = false,
                        QuestionId = 8
                    },
                    new QuestionOption
                    {
                        Text = "new",
                        IsCorrect = false,
                        QuestionId = 8
                    },

                    // Question 9
                    new QuestionOption
                    {
                        Text = "SQL",
                        IsCorrect = true,
                        QuestionId = 9
                    },
                    new QuestionOption
                    {
                        Text = "CSS",
                        IsCorrect = false,
                        QuestionId = 9
                    },
                    new QuestionOption
                    {
                        Text = "HTML",
                        IsCorrect = false,
                        QuestionId = 9
                    },

                    // Question 10
                    new QuestionOption
                    {
                        Text =
                            "To uniquely identify a row",
                        IsCorrect = true,
                        QuestionId = 10
                    },
                    new QuestionOption
                    {
                        Text =
                            "To change the database color",
                        IsCorrect = false,
                        QuestionId = 10
                    },
                    new QuestionOption
                    {
                        Text =
                            "To delete every row",
                        IsCorrect = false,
                        QuestionId = 10
                    }
                };

                context.AddRange(questionOptions);
                context.SaveChanges();
            }

            // Add UserChallenges
            if (!context.UserChallenges.Any())
            {
                var userChallenges =
                    new List<UserChallenge>
                {
                    // Alice has taken HTML Quiz
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 1
                    },

                    // Bob has taken HTML Quiz
                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 1
                    },

                    // Charlie has taken HTML Quiz
                    new UserChallenge
                    {
                        UserId = 4,
                        ChallengeId = 1
                    },

                    // Emma has taken Programming Challenge
                    new UserChallenge
                    {
                        UserId = 5,
                        ChallengeId = 2
                    },

                    // David has taken Web Development
                    new UserChallenge
                    {
                        UserId = 6,
                        ChallengeId = 3
                    },

                    // Alice has also taken Database Quiz
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 5
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
                    // Alice - first HTML attempt
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

                    // Alice - best HTML attempt
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

                    // Bob - same score as Alice but slower
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

                    // Charlie - HTML Quiz
                    new ChallengeAttempt
                    {
                        UserChallengeId = 3,
                        Score = 10,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddHours(-4)
                                .AddMinutes(-5),
                        CompletedAt =
                            DateTime.Now
                                .AddHours(-4)
                    },

                    // Emma - Programming Challenge
                    new ChallengeAttempt
                    {
                        UserChallengeId = 4,
                        Score = 20,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddHours(-3)
                                .AddMinutes(-6),
                        CompletedAt =
                            DateTime.Now
                                .AddHours(-3)
                    },

                    // David - Web Development
                    new ChallengeAttempt
                    {
                        UserChallengeId = 5,
                        Score = 10,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddHours(-2)
                                .AddMinutes(-7),
                        CompletedAt =
                            DateTime.Now
                                .AddHours(-2)
                    },

                    // Alice - Database Quiz
                    new ChallengeAttempt
                    {
                        UserChallengeId = 6,
                        Score = 20,
                        Completed = true,
                        StartedAt =
                            DateTime.Now
                                .AddHours(-1)
                                .AddMinutes(-4),
                        CompletedAt =
                            DateTime.Now
                                .AddHours(-1)
                    }
                };

                context.AddRange(challengeAttempts);
                context.SaveChanges();
            }
        }
    }
}