using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
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


            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GamificationPlatform.DAL.DBInit");

            logger.LogInformation("Development database initialization started.");
            try
            {
                SeedDatabase(context);
                logger.LogInformation("Development database initialization completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Development database initialization failed.");
                throw;
            }
        }

        private static void SeedDatabase(ChallengeDbContext context)
        {
            // Delete the existing database and create it again.
            // Useful while testing with dummy data.
            context.Database.EnsureDeleted();
            context.Database.Migrate();


            // --------------------------------
            // Users
            // --------------------------------

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


            // Gives all dummy users the same
            // password for easier testing.
            var passwordHasher =
                new PasswordHasher<User>();


            foreach (var user in users)
            {
                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        "Password123!");
            }


            context.Users.AddRange(users);
            context.SaveChanges();


            // --------------------------------
            // Challenges
            // --------------------------------

            var challenges = new List<Challenge>
            {
                // Alice
                new Challenge
                {
                    Title = "HTML Quiz",

                    Description =
                        "Test your knowledge of HTML.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_2.png",

                    TimeLimitMinutes = 10,

                    IsPublished = true,

                    CreatedByUserId = 2
                },


                // Bob
                new Challenge
                {
                    Title = "Programming Challenge",

                    Description =
                        "Test your basic programming knowledge.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_3.png",

                    TimeLimitMinutes = 20,

                    IsPublished = true,

                    CreatedByUserId = 3
                },


                // Charlie
                new Challenge
                {
                    Title = "Web Development",

                    Description =
                        "Answer questions about web development.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_4.png",

                    TimeLimitMinutes = 30,

                    IsPublished = true,

                    CreatedByUserId = 4
                },


                // Emma
                new Challenge
                {
                    Title = "C# Quiz",

                    Description =
                        "Test your knowledge of C#.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_5.png",

                    TimeLimitMinutes = 20,

                    IsPublished = true,

                    CreatedByUserId = 5
                },


                // David
                new Challenge
                {
                    Title = "Database Quiz",

                    Description =
                        "Test your database knowledge.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_6.png",

                    TimeLimitMinutes = null,

                    IsPublished = true,

                    CreatedByUserId = 6
                },


                // Alice
                new Challenge
                {
                    Title = "CSS Challenge",

                    Description =
                        "Test your knowledge of CSS and styling.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_7.png",

                    TimeLimitMinutes = 10,

                    IsPublished = true,

                    CreatedByUserId = 2
                },


                // Bob
                new Challenge
                {
                    Title = "ASP.NET Core Quiz",

                    Description =
                        "Test your ASP.NET Core knowledge.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_11.png",

                    TimeLimitMinutes = 60,

                    IsPublished = true,

                    CreatedByUserId = 3
                },


                // Charlie
                // This challenge is intentionally a draft.
                new Challenge
                {
                    Title = "JavaScript Quiz",

                    Description =
                        "A JavaScript quiz currently being created.",

                    MaxPoints = 0,

                    ImageUrl =
                        "/images/Question_9.png",

                    TimeLimitMinutes = 30,

                    IsPublished = false,

                    CreatedByUserId = 4
                }
            };


            context.Challenges.AddRange(challenges);
            context.SaveChanges();


            // --------------------------------
            // Questions
            // --------------------------------

            var questions = new List<Question>
            {
                // --------------------------------
                // HTML Quiz - Challenge 1
                // --------------------------------

                new Question
                {
                    Title =
                        "What does HTML stand for?",

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

                new Question
                {
                    Title =
                        "Which HTML tag creates a link?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 1
                },


                // --------------------------------
                // Programming Challenge - Challenge 2
                // --------------------------------

                new Question
                {
                    Title =
                        "Which keyword can create a variable in C#?",

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

                new Question
                {
                    Title =
                        "What is a loop used for?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 2
                },


                // --------------------------------
                // Web Development - Challenge 3
                // --------------------------------

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
                    Title =
                        "What does CSS stand for?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 3
                },

                new Question
                {
                    Title =
                        "Which language runs directly in the browser?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 3
                },


                // --------------------------------
                // C# Quiz - Challenge 4
                // --------------------------------

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

                new Question
                {
                    Title =
                        "Which type stores true or false values?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 4
                },

                new Question
                {
                    Title =
                        "Which keyword creates a new object?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 4
                },


                // --------------------------------
                // Database Quiz - Challenge 5
                // --------------------------------

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
                },

                new Question
                {
                    Title =
                        "Which SQL command retrieves data?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 5
                },


                // --------------------------------
                // CSS Challenge - Challenge 6
                // --------------------------------

                new Question
                {
                    Title =
                        "Which CSS property changes text color?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 6
                },

                new Question
                {
                    Title =
                        "Which CSS property changes the background color?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 6
                },

                new Question
                {
                    Title =
                        "Which selector targets an element by id?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 6
                },


                // --------------------------------
                // ASP.NET Core Quiz - Challenge 7
                // --------------------------------

                new Question
                {
                    Title =
                        "What does MVC stand for?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 7
                },

                new Question
                {
                    Title =
                        "Which part of MVC handles user requests?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 7
                },

                new Question
                {
                    Title =
                        "Which part of MVC displays the user interface?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 7
                },

                new Question
                {
                    Title =
                        "What is Entity Framework Core commonly used for?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 7
                },


                // --------------------------------
                // JavaScript Quiz - Challenge 8
                // Draft challenge
                // --------------------------------

                new Question
                {
                    Title =
                        "Which keyword declares a variable in JavaScript?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 8
                },

                new Question
                {
                    Title =
                        "Which function writes a message to the browser console?",

                    Description =
                        "Choose the correct answer.",

                    Points = 10,

                    ChallengeId = 8
                }
            };


            context.Questions.AddRange(questions);
            context.SaveChanges();


            // --------------------------------
            // Question Options
            // --------------------------------

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
                        Text = "<a>",
                        IsCorrect = true,
                        QuestionId = 3
                    },

                    new QuestionOption
                    {
                        Text = "<link>",
                        IsCorrect = false,
                        QuestionId = 3
                    },

                    new QuestionOption
                    {
                        Text = "<url>",
                        IsCorrect = false,
                        QuestionId = 3
                    },


                    // Question 4
                    new QuestionOption
                    {
                        Text = "var",
                        IsCorrect = true,
                        QuestionId = 4
                    },

                    new QuestionOption
                    {
                        Text = "variable",
                        IsCorrect = false,
                        QuestionId = 4
                    },

                    new QuestionOption
                    {
                        Text = "define",
                        IsCorrect = false,
                        QuestionId = 4
                    },


                    // Question 5
                    new QuestionOption
                    {
                        Text = ";",
                        IsCorrect = true,
                        QuestionId = 5
                    },

                    new QuestionOption
                    {
                        Text = ":",
                        IsCorrect = false,
                        QuestionId = 5
                    },

                    new QuestionOption
                    {
                        Text = "#",
                        IsCorrect = false,
                        QuestionId = 5
                    },


                    // Question 6
                    new QuestionOption
                    {
                        Text = "To repeat code",
                        IsCorrect = true,
                        QuestionId = 6
                    },

                    new QuestionOption
                    {
                        Text = "To delete a variable",
                        IsCorrect = false,
                        QuestionId = 6
                    },

                    new QuestionOption
                    {
                        Text = "To create a database",
                        IsCorrect = false,
                        QuestionId = 6
                    },


                    // Question 7
                    new QuestionOption
                    {
                        Text = "CSS",
                        IsCorrect = true,
                        QuestionId = 7
                    },

                    new QuestionOption
                    {
                        Text = "HTML",
                        IsCorrect = false,
                        QuestionId = 7
                    },

                    new QuestionOption
                    {
                        Text = "SQL",
                        IsCorrect = false,
                        QuestionId = 7
                    },


                    // Question 8
                    new QuestionOption
                    {
                        Text = "Cascading Style Sheets",
                        IsCorrect = true,
                        QuestionId = 8
                    },

                    new QuestionOption
                    {
                        Text = "Computer Style System",
                        IsCorrect = false,
                        QuestionId = 8
                    },

                    new QuestionOption
                    {
                        Text = "Creative Style Sheets",
                        IsCorrect = false,
                        QuestionId = 8
                    },


                    // Question 9
                    new QuestionOption
                    {
                        Text = "JavaScript",
                        IsCorrect = true,
                        QuestionId = 9
                    },

                    new QuestionOption
                    {
                        Text = "SQL",
                        IsCorrect = false,
                        QuestionId = 9
                    },

                    new QuestionOption
                    {
                        Text = "C# only",
                        IsCorrect = false,
                        QuestionId = 9
                    },


                    // Question 10
                    new QuestionOption
                    {
                        Text = "int",
                        IsCorrect = true,
                        QuestionId = 10
                    },

                    new QuestionOption
                    {
                        Text = "string",
                        IsCorrect = false,
                        QuestionId = 10
                    },

                    new QuestionOption
                    {
                        Text = "bool",
                        IsCorrect = false,
                        QuestionId = 10
                    },


                    // Question 11
                    new QuestionOption
                    {
                        Text = "class",
                        IsCorrect = true,
                        QuestionId = 11
                    },

                    new QuestionOption
                    {
                        Text = "object",
                        IsCorrect = false,
                        QuestionId = 11
                    },

                    new QuestionOption
                    {
                        Text = "create",
                        IsCorrect = false,
                        QuestionId = 11
                    },


                    // Question 12
                    new QuestionOption
                    {
                        Text = "bool",
                        IsCorrect = true,
                        QuestionId = 12
                    },

                    new QuestionOption
                    {
                        Text = "int",
                        IsCorrect = false,
                        QuestionId = 12
                    },

                    new QuestionOption
                    {
                        Text = "string",
                        IsCorrect = false,
                        QuestionId = 12
                    },


                    // Question 13
                    new QuestionOption
                    {
                        Text = "new",
                        IsCorrect = true,
                        QuestionId = 13
                    },

                    new QuestionOption
                    {
                        Text = "create",
                        IsCorrect = false,
                        QuestionId = 13
                    },

                    new QuestionOption
                    {
                        Text = "object",
                        IsCorrect = false,
                        QuestionId = 13
                    },


                    // Question 14
                    new QuestionOption
                    {
                        Text = "SQL",
                        IsCorrect = true,
                        QuestionId = 14
                    },

                    new QuestionOption
                    {
                        Text = "CSS",
                        IsCorrect = false,
                        QuestionId = 14
                    },

                    new QuestionOption
                    {
                        Text = "HTML",
                        IsCorrect = false,
                        QuestionId = 14
                    },


                    // Question 15
                    new QuestionOption
                    {
                        Text = "To uniquely identify a row",
                        IsCorrect = true,
                        QuestionId = 15
                    },

                    new QuestionOption
                    {
                        Text = "To change the database color",
                        IsCorrect = false,
                        QuestionId = 15
                    },

                    new QuestionOption
                    {
                        Text = "To delete every row",
                        IsCorrect = false,
                        QuestionId = 15
                    },


                    // Question 16
                    new QuestionOption
                    {
                        Text = "SELECT",
                        IsCorrect = true,
                        QuestionId = 16
                    },

                    new QuestionOption
                    {
                        Text = "DELETE",
                        IsCorrect = false,
                        QuestionId = 16
                    },

                    new QuestionOption
                    {
                        Text = "DROP",
                        IsCorrect = false,
                        QuestionId = 16
                    },


                    // Question 17
                    new QuestionOption
                    {
                        Text = "color",
                        IsCorrect = true,
                        QuestionId = 17
                    },

                    new QuestionOption
                    {
                        Text = "text-color",
                        IsCorrect = false,
                        QuestionId = 17
                    },

                    new QuestionOption
                    {
                        Text = "font-color",
                        IsCorrect = false,
                        QuestionId = 17
                    },


                    // Question 18
                    new QuestionOption
                    {
                        Text = "background-color",
                        IsCorrect = true,
                        QuestionId = 18
                    },

                    new QuestionOption
                    {
                        Text = "background",
                        IsCorrect = false,
                        QuestionId = 18
                    },

                    new QuestionOption
                    {
                        Text = "page-color",
                        IsCorrect = false,
                        QuestionId = 18
                    },


                    // Question 19
                    new QuestionOption
                    {
                        Text = "#",
                        IsCorrect = true,
                        QuestionId = 19
                    },

                    new QuestionOption
                    {
                        Text = ".",
                        IsCorrect = false,
                        QuestionId = 19
                    },

                    new QuestionOption
                    {
                        Text = "*",
                        IsCorrect = false,
                        QuestionId = 19
                    },


                    // Question 20
                    new QuestionOption
                    {
                        Text = "Model View Controller",
                        IsCorrect = true,
                        QuestionId = 20
                    },

                    new QuestionOption
                    {
                        Text = "Main View Component",
                        IsCorrect = false,
                        QuestionId = 20
                    },

                    new QuestionOption
                    {
                        Text = "Model Variable Class",
                        IsCorrect = false,
                        QuestionId = 20
                    },


                    // Question 21
                    new QuestionOption
                    {
                        Text = "Controller",
                        IsCorrect = true,
                        QuestionId = 21
                    },

                    new QuestionOption
                    {
                        Text = "View",
                        IsCorrect = false,
                        QuestionId = 21
                    },

                    new QuestionOption
                    {
                        Text = "Model",
                        IsCorrect = false,
                        QuestionId = 21
                    },


                    // Question 22
                    new QuestionOption
                    {
                        Text = "View",
                        IsCorrect = true,
                        QuestionId = 22
                    },

                    new QuestionOption
                    {
                        Text = "Controller",
                        IsCorrect = false,
                        QuestionId = 22
                    },

                    new QuestionOption
                    {
                        Text = "Database",
                        IsCorrect = false,
                        QuestionId = 22
                    },


                    // Question 23
                    new QuestionOption
                    {
                        Text = "Working with databases",
                        IsCorrect = true,
                        QuestionId = 23
                    },

                    new QuestionOption
                    {
                        Text = "Creating CSS styles",
                        IsCorrect = false,
                        QuestionId = 23
                    },

                    new QuestionOption
                    {
                        Text = "Editing images",
                        IsCorrect = false,
                        QuestionId = 23
                    },


                    // Question 24
                    new QuestionOption
                    {
                        Text = "let",
                        IsCorrect = true,
                        QuestionId = 24
                    },

                    new QuestionOption
                    {
                        Text = "int",
                        IsCorrect = false,
                        QuestionId = 24
                    },

                    new QuestionOption
                    {
                        Text = "string",
                        IsCorrect = false,
                        QuestionId = 24
                    },


                    // Question 25
                    new QuestionOption
                    {
                        Text = "console.log()",
                        IsCorrect = true,
                        QuestionId = 25
                    },

                    new QuestionOption
                    {
                        Text = "print()",
                        IsCorrect = false,
                        QuestionId = 25
                    },

                    new QuestionOption
                    {
                        Text = "Console.WriteLine()",
                        IsCorrect = false,
                        QuestionId = 25
                    }
                };


            context.QuestionOptions.AddRange(
                questionOptions);

            context.SaveChanges();


            // --------------------------------
            // UserChallenges
            // --------------------------------

            var userChallenges =
                new List<UserChallenge>
                {
                    // Alice - HTML Quiz
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 1
                    },

                    // Bob - HTML Quiz
                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 1
                    },

                    // Charlie - HTML Quiz
                    new UserChallenge
                    {
                        UserId = 4,
                        ChallengeId = 1
                    },

                    // Emma - HTML Quiz
                    new UserChallenge
                    {
                        UserId = 5,
                        ChallengeId = 1
                    },

                    // David - HTML Quiz
                    new UserChallenge
                    {
                        UserId = 6,
                        ChallengeId = 1
                    },


                    // Alice - Programming
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 2
                    },

                    // Emma - Programming
                    new UserChallenge
                    {
                        UserId = 5,
                        ChallengeId = 2
                    },

                    // David - Programming
                    new UserChallenge
                    {
                        UserId = 6,
                        ChallengeId = 2
                    },


                    // Alice - Web Development
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 3
                    },

                    // Bob - Web Development
                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 3
                    },

                    // David - Web Development
                    new UserChallenge
                    {
                        UserId = 6,
                        ChallengeId = 3
                    },


                    // Alice - C# Quiz
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 4
                    },

                    // Bob - C# Quiz
                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 4
                    },


                    // Alice - Database Quiz
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = 5
                    },

                    // Charlie - Database Quiz
                    new UserChallenge
                    {
                        UserId = 4,
                        ChallengeId = 5
                    },


                    // Bob - CSS Challenge
                    new UserChallenge
                    {
                        UserId = 3,
                        ChallengeId = 6
                    },

                    // Emma - CSS Challenge
                    new UserChallenge
                    {
                        UserId = 5,
                        ChallengeId = 6
                    },


                    // Charlie - ASP.NET Core
                    new UserChallenge
                    {
                        UserId = 4,
                        ChallengeId = 7
                    },

                    // David - ASP.NET Core
                    new UserChallenge
                    {
                        UserId = 6,
                        ChallengeId = 7
                    }
                };


            context.UserChallenges.AddRange(
                userChallenges);

            context.SaveChanges();


            // --------------------------------
            // Challenge Attempts
            // --------------------------------

            var now = DateTime.Now;


            var challengeAttempts =
                new List<ChallengeAttempt>
                {
                    // --------------------------------
                    // HTML Quiz
                    // --------------------------------

                    // Alice - first attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 1,
                        Score = 10,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-5)
                               .AddMinutes(-8),

                        CompletedAt =
                            now.AddDays(-5)
                    },

                    // Alice - second attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 1,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-3)
                               .AddMinutes(-6),

                        CompletedAt =
                            now.AddDays(-3)
                    },

                    // Alice - latest and best attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 1,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-1)
                               .AddMinutes(-4),

                        CompletedAt =
                            now.AddDays(-1)
                    },


                    // Bob - perfect but slower
                    new ChallengeAttempt
                    {
                        UserChallengeId = 2,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-2)
                               .AddMinutes(-7),

                        CompletedAt =
                            now.AddDays(-2)
                    },


                    // Charlie
                    new ChallengeAttempt
                    {
                        UserChallengeId = 3,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-20)
                               .AddMinutes(-5),

                        CompletedAt =
                            now.AddHours(-20)
                    },


                    // Emma
                    new ChallengeAttempt
                    {
                        UserChallengeId = 4,
                        Score = 10,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-10)
                               .AddMinutes(-6),

                        CompletedAt =
                            now.AddHours(-10)
                    },


                    // David
                    new ChallengeAttempt
                    {
                        UserChallengeId = 5,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-8)
                               .AddMinutes(-3),

                        CompletedAt =
                            now.AddHours(-8)
                    },


                    // --------------------------------
                    // Programming Challenge
                    // --------------------------------

                    // Alice
                    new ChallengeAttempt
                    {
                        UserChallengeId = 6,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-3)
                               .AddMinutes(-8),

                        CompletedAt =
                            now.AddDays(-3)
                    },

                    // Emma
                    new ChallengeAttempt
                    {
                        UserChallengeId = 7,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-2)
                               .AddMinutes(-6),

                        CompletedAt =
                            now.AddDays(-2)
                    },

                    // David
                    new ChallengeAttempt
                    {
                        UserChallengeId = 8,
                        Score = 10,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-1)
                               .AddMinutes(-9),

                        CompletedAt =
                            now.AddDays(-1)
                    },


                    // --------------------------------
                    // Web Development
                    // --------------------------------

                    // Alice
                    new ChallengeAttempt
                    {
                        UserChallengeId = 9,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-4)
                               .AddMinutes(-5),

                        CompletedAt =
                            now.AddDays(-4)
                    },

                    // Bob
                    new ChallengeAttempt
                    {
                        UserChallengeId = 10,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-2)
                               .AddMinutes(-7),

                        CompletedAt =
                            now.AddDays(-2)
                    },

                    // David
                    new ChallengeAttempt
                    {
                        UserChallengeId = 11,
                        Score = 10,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-12)
                               .AddMinutes(-8),

                        CompletedAt =
                            now.AddHours(-12)
                    },


                    // --------------------------------
                    // C# Quiz
                    // --------------------------------

                    // Alice
                    new ChallengeAttempt
                    {
                        UserChallengeId = 12,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-2)
                               .AddMinutes(-9),

                        CompletedAt =
                            now.AddDays(-2)
                    },

                    // Alice - latest, better attempt
                    new ChallengeAttempt
                    {
                        UserChallengeId = 12,
                        Score = 40,
                        Completed = true,

                        StartedAt =
                            now.AddDays(-1)
                               .AddMinutes(-6),

                        CompletedAt =
                            now.AddDays(-1)
                    },

                    // Bob
                    new ChallengeAttempt
                    {
                        UserChallengeId = 13,
                        Score = 40,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-18)
                               .AddMinutes(-8),

                        CompletedAt =
                            now.AddHours(-18)
                    },


                    // --------------------------------
                    // Database Quiz
                    // --------------------------------

                    // Alice
                    new ChallengeAttempt
                    {
                        UserChallengeId = 14,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-9)
                               .AddMinutes(-4),

                        CompletedAt =
                            now.AddHours(-9)
                    },

                    // Charlie
                    new ChallengeAttempt
                    {
                        UserChallengeId = 15,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-7)
                               .AddMinutes(-5),

                        CompletedAt =
                            now.AddHours(-7)
                    },


                    // --------------------------------
                    // CSS Challenge
                    // --------------------------------

                    // Bob
                    new ChallengeAttempt
                    {
                        UserChallengeId = 16,
                        Score = 20,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-6)
                               .AddMinutes(-5),

                        CompletedAt =
                            now.AddHours(-6)
                    },

                    // Emma
                    new ChallengeAttempt
                    {
                        UserChallengeId = 17,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-5)
                               .AddMinutes(-3),

                        CompletedAt =
                            now.AddHours(-5)
                    },


                    // --------------------------------
                    // ASP.NET Core Quiz
                    // --------------------------------

                    // Charlie
                    new ChallengeAttempt
                    {
                        UserChallengeId = 18,
                        Score = 30,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-4)
                               .AddMinutes(-10),

                        CompletedAt =
                            now.AddHours(-4)
                    },

                    // David
                    new ChallengeAttempt
                    {
                        UserChallengeId = 19,
                        Score = 40,
                        Completed = true,

                        StartedAt =
                            now.AddHours(-2)
                               .AddMinutes(-7),

                        CompletedAt =
                            now.AddHours(-2)
                    }
                };


            context.ChallengeAttempts.AddRange(
                challengeAttempts);

            context.SaveChanges();


            // --------------------------------
            // Attempt Answers
            // --------------------------------
            //
            // Adds saved answers to some completed
            // attempts so Attempt Details can also
            // be tested with the dummy database.


            var attemptAnswers =
                new List<AttemptAnswer>
                {
                    // --------------------------------
                    // Alice - HTML Attempt 1
                    // Score: 10 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 1,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 1,
                        QuestionId = 2,
                        SelectedOptionId = 5
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 1,
                        QuestionId = 3,
                        SelectedOptionId = 8
                    },


                    // --------------------------------
                    // Alice - HTML Attempt 2
                    // Score: 20 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 2,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 2,
                        QuestionId = 2,
                        SelectedOptionId = 4
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 2,
                        QuestionId = 3,
                        SelectedOptionId = 8
                    },


                    // --------------------------------
                    // Alice - HTML Attempt 3
                    // Score: 30 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 3,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 3,
                        QuestionId = 2,
                        SelectedOptionId = 4
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 3,
                        QuestionId = 3,
                        SelectedOptionId = 7
                    },


                    // --------------------------------
                    // Bob - HTML
                    // Score: 30 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 4,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 4,
                        QuestionId = 2,
                        SelectedOptionId = 4
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 4,
                        QuestionId = 3,
                        SelectedOptionId = 7
                    },


                    // --------------------------------
                    // Charlie - HTML
                    // Score: 20 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 5,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 5,
                        QuestionId = 2,
                        SelectedOptionId = 4
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 5,
                        QuestionId = 3,
                        SelectedOptionId = 8
                    },


                    // --------------------------------
                    // Emma - HTML
                    // Score: 10 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 6,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 6,
                        QuestionId = 2,
                        SelectedOptionId = 5
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 6,
                        QuestionId = 3,
                        SelectedOptionId = 8
                    },


                    // --------------------------------
                    // David - HTML
                    // Score: 30 / 30
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 7,
                        QuestionId = 1,
                        SelectedOptionId = 1
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 7,
                        QuestionId = 2,
                        SelectedOptionId = 4
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 7,
                        QuestionId = 3,
                        SelectedOptionId = 7
                    },


                    // --------------------------------
                    // Alice - C# first attempt
                    // Score: 30 / 40
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 14,
                        QuestionId = 10,
                        SelectedOptionId = 28
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 14,
                        QuestionId = 11,
                        SelectedOptionId = 31
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 14,
                        QuestionId = 12,
                        SelectedOptionId = 34
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 14,
                        QuestionId = 13,
                        SelectedOptionId = 38
                    },


                    // --------------------------------
                    // Alice - C# second attempt
                    // Score: 40 / 40
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 15,
                        QuestionId = 10,
                        SelectedOptionId = 28
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 15,
                        QuestionId = 11,
                        SelectedOptionId = 31
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 15,
                        QuestionId = 12,
                        SelectedOptionId = 34
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 15,
                        QuestionId = 13,
                        SelectedOptionId = 37
                    },


                    // --------------------------------
                    // Bob - C#
                    // Score: 40 / 40
                    // --------------------------------

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 16,
                        QuestionId = 10,
                        SelectedOptionId = 28
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 16,
                        QuestionId = 11,
                        SelectedOptionId = 31
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 16,
                        QuestionId = 12,
                        SelectedOptionId = 34
                    },

                    new AttemptAnswer
                    {
                        ChallengeAttemptId = 16,
                        QuestionId = 13,
                        SelectedOptionId = 37
                    }
                };


            context.AttemptAnswers.AddRange(
                attemptAnswers);

            context.SaveChanges();
        }
    }
}
