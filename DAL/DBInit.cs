using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
namespace GamificationPlatform.DAL
{
    public static class DBInit
    {

        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var serviceScope =
                app.ApplicationServices.CreateScope();

            ChallengeDbContext context =
                serviceScope.ServiceProvider
                    .GetRequiredService<ChallengeDbContext>();

            var userManager = serviceScope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var roleManager = serviceScope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<int>>>();

            var achievementService =
                serviceScope.ServiceProvider
                    .GetRequiredService<IAchievementService>();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GamificationPlatform.DAL.DBInit");

            logger.LogInformation(
                "Development database initialization started.");

            try
            {
                await SeedDatabaseAsync(context, userManager, roleManager);

                // Update achievements for the seeded users.
                var userIds =
                    await context.Users
                        .Select(u => u.Id)
                        .ToListAsync();

                foreach (var userId in userIds)
                {
                    await achievementService
                        .CheckAchievementsAsync(userId);
                }

                logger.LogInformation(
                    "Development database initialization completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Development database initialization failed.");

                throw;
            }
        }

        private static async Task SeedDatabaseAsync(
            ChallengeDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Reset the development database.
            context.Database.EnsureDeleted();
            context.Database.Migrate();
            // Remove uploaded images from the previous database.
            ClearUploadedImages();
            // --------------------------------
            // Users
            // --------------------------------
            var users = new List<User>
            {
                new User { Id = 1, UserName = "admin", Email = "admin@example.com"},
                new User { Id = 2, UserName = "Alice", Email = "alice@example.com" },
                new User { Id = 3, UserName = "Bob", Email = "bob@example.com" },
                new User { Id = 4, UserName = "Charlie", Email = "charlie@example.com" },
                new User { Id = 5, UserName = "Emma", Email = "emma@example.com" },
                new User { Id = 6, UserName = "David", Email = "david@example.com" }
            };

            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<int>("Admin"));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create {user.UserName}: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            var adminRoleResult = await userManager.AddToRoleAsync(users[0], "Admin");
            if (!adminRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", adminRoleResult.Errors.Select(e => e.Description)));
            }

            // --------------------------------
            // Achievements
            // --------------------------------
            var achievements = new List<Achievement>
            {
                // --------------------------------
                // Challenge Completion
                // --------------------------------
                new Achievement
                {
                    Name = "First Challenge",
                    Description =
                        "Complete your first challenge.",
                    Points = 10,
                    Type = "ChallengesCompleted",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Getting the Hang of It",
                    Description =
                        "Complete 5 challenges.",
                    Points = 10,
                    Type = "ChallengesCompleted",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Challenge Hunter",
                    Description =
                        "Complete 10 challenges.",
                    Points = 15,
                    Type = "ChallengesCompleted",
                    RequirementValue = 10,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Dedicated Learner",
                    Description =
                        "Complete 20 challenges.",
                    Points = 20,
                    Type = "ChallengesCompleted",
                    RequirementValue = 20,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Score
                // --------------------------------
                new Achievement
                {
                    Name = "Halfway There",
                    Description =
                        "Score at least 50% on a challenge.",
                    Points = 5,
                    Type = "ScorePercentage",
                    RequirementValue = 50,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Getting Better",
                    Description =
                        "Score at least 60% on a challenge.",
                    Points = 5,
                    Type = "ScorePercentage",
                    RequirementValue = 60,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Good Job!",
                    Description =
                        "Score at least 70% on a challenge.",
                    Points = 10,
                    Type = "ScorePercentage",
                    RequirementValue = 70,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Great Job!",
                    Description =
                        "Score at least 80% on a challenge.",
                    Points = 10,
                    Type = "ScorePercentage",
                    RequirementValue = 80,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Almost Perfect",
                    Description =
                        "Score at least 90% on a challenge.",
                    Points = 15,
                    Type = "ScorePercentage",
                    RequirementValue = 90,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Perfect Score",
                    Description =
                        "Score 100% on a challenge.",
                    Points = 20,
                    Type = "ScorePercentage",
                    RequirementValue = 100,
                    IsActive = true
                },

                // --------------------------------
                // Practice
                // --------------------------------
                new Achievement
                {
                    Name = "Second Time's the Charm",
                    Description =
                        "Improve your score on your second attempt.",
                    Points = 10,
                    Type = "SecondAttemptImprovement",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Practice Makes Perfect",
                    Description =
                        "Complete the same challenge 5 times.",
                    Points = 15,
                    Type = "SameChallengeAttempts",
                    RequirementValue = 5,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Creator
                // --------------------------------
                new Achievement
                {
                    Name = "Challenge Creator",
                    Description =
                        "Create your first challenge.",
                    Points = 5,
                    Type = "ChallengesCreated",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Content Creator",
                    Description =
                        "Create 5 challenges.",
                    Points = 15,
                    Type = "ChallengesCreated",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "First Participant",
                    Description =
                        "Have another user complete one of your challenges.",
                    Points = 10,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Popular Creator",
                    Description =
                        "Have 5 different users complete challenges you created.",
                    Points = 10,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Community Favorite",
                    Description =
                        "Have 10 different users complete challenges you created.",
                    Points = 15,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 10,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Leaderboard
                // --------------------------------
                new Achievement
                {
                    Name = "So Close, Yet So Far",
                    Description =
                        "Reach 4th place on a Challenge Leaderboard.",
                    Points = 5,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 4,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "On the Podium",
                    Description =
                        "Reach 3rd place on a Challenge Leaderboard.",
                    Points = 10,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 3,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Silver Medalist",
                    Description =
                        "Reach 2nd place on a Challenge Leaderboard.",
                    Points = 15,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 2,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Challenge Champion",
                    Description =
                        "Reach 1st place on a Challenge Leaderboard.",
                    Points = 20,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 1,
                    IsActive = true
                },

                // --------------------------------
                // Core Leaderboard
                // --------------------------------
                new Achievement
                {
                    Name = "Core Contender",
                    Description =
                        "Reach 4th place on the Core Leaderboard.",
                    Points = 5,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 4,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Bronze",
                    Description =
                        "Reach 3rd place on the Core Leaderboard.",
                    Points = 10,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 3,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Silver",
                    Description =
                        "Reach 2nd place on the Core Leaderboard.",
                    Points = 15,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 2,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Champion",
                    Description =
                        "Reach 1st place on the Core Leaderboard.",
                    Points = 20,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 1,
                    IsActive = true
                },
                
                // --------------------------------
                // Goals
                // --------------------------------
                new Achievement
                {
                    Name = "Bronze Achiever",
                    Description =
                        "Reach the Bronze milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 20,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Silver Achiever",
                    Description =
                        "Reach the Silver milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 40,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Gold Achiever",
                    Description =
                        "Reach the Gold milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 60,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Diamond Achiever",
                    Description =
                        "Reach the Diamond milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 80,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Legendary",
                    Description =
                        "Reach the Legend milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 100,
                    IsActive = true,
                    IsGoal = true
                }
            };

            context.Achievements.AddRange(achievements);
            context.SaveChanges();


            // --------------------------------
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
                // Community challenge
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
                },
                // Question 26 - Challenge 9
                new Question
                {
                    Title =
                        "Which keyword defines a function in Python?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 9
                },
                // Question 27 - Challenge 9
                new Question
                {
                    Title =
                        "Which symbol starts a comment in Python?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 9
                },
                // Question 28 - Challenge 9
                new Question
                {
                    Title =
                        "Which function prints text in Python?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 9
                },
                // Question 29 - Challenge 10
                new Question
                {
                    Title =
                        "Which SQL command adds a new row to a table?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 10
                },
                // Question 30 - Challenge 10
                new Question
                {
                    Title =
                        "Which SQL clause filters rows?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 10
                },
                // Question 31 - Challenge 10
                new Question
                {
                    Title =
                        "Which SQL command changes existing data?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 10
                },
                // Question 32 - Challenge 11
                new Question
                {
                    Title =
                        "Which command creates a new Git repository?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 11
                },
                // Question 33 - Challenge 11
                new Question
                {
                    Title =
                        "Which command records staged changes?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 11
                },
                // Question 34 - Challenge 11
                new Question
                {
                    Title =
                        "Which command copies a remote repository?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 11
                },
                // Question 35 - Challenge 12
                new Question
                {
                    Title =
                        "What does MFA stand for?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 12
                },
                // Question 36 - Challenge 12
                new Question
                {
                    Title =
                        "Which password is the strongest?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 12
                },
                // Question 37 - Challenge 12
                new Question
                {
                    Title =
                        "What does HTTPS help protect?",
                    Description =
                        "Choose the correct answer.",
                    Points = 10,
                    ChallengeId = 12
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
                    },
                    // Question 26
                    new QuestionOption
                    {
                        Text = "def",
                        IsCorrect = true,
                        QuestionId = 26
                    },
                    new QuestionOption
                    {
                        Text = "function",
                        IsCorrect = false,
                        QuestionId = 26
                    },
                    new QuestionOption
                    {
                        Text = "func",
                        IsCorrect = false,
                        QuestionId = 26
                    },
                    // Question 27
                    new QuestionOption
                    {
                        Text = "#",
                        IsCorrect = true,
                        QuestionId = 27
                    },
                    new QuestionOption
                    {
                        Text = "//",
                        IsCorrect = false,
                        QuestionId = 27
                    },
                    new QuestionOption
                    {
                        Text = "<!--",
                        IsCorrect = false,
                        QuestionId = 27
                    },
                    // Question 28
                    new QuestionOption
                    {
                        Text = "print()",
                        IsCorrect = true,
                        QuestionId = 28
                    },
                    new QuestionOption
                    {
                        Text = "Console.WriteLine()",
                        IsCorrect = false,
                        QuestionId = 28
                    },
                    new QuestionOption
                    {
                        Text = "echo()",
                        IsCorrect = false,
                        QuestionId = 28
                    },
                    // Question 29
                    new QuestionOption
                    {
                        Text = "INSERT",
                        IsCorrect = true,
                        QuestionId = 29
                    },
                    new QuestionOption
                    {
                        Text = "SELECT",
                        IsCorrect = false,
                        QuestionId = 29
                    },
                    new QuestionOption
                    {
                        Text = "DROP",
                        IsCorrect = false,
                        QuestionId = 29
                    },
                    // Question 30
                    new QuestionOption
                    {
                        Text = "WHERE",
                        IsCorrect = true,
                        QuestionId = 30
                    },
                    new QuestionOption
                    {
                        Text = "ORDER",
                        IsCorrect = false,
                        QuestionId = 30
                    },
                    new QuestionOption
                    {
                        Text = "TABLE",
                        IsCorrect = false,
                        QuestionId = 30
                    },
                    // Question 31
                    new QuestionOption
                    {
                        Text = "UPDATE",
                        IsCorrect = true,
                        QuestionId = 31
                    },
                    new QuestionOption
                    {
                        Text = "CREATE",
                        IsCorrect = false,
                        QuestionId = 31
                    },
                    new QuestionOption
                    {
                        Text = "SELECT",
                        IsCorrect = false,
                        QuestionId = 31
                    },
                    // Question 32
                    new QuestionOption
                    {
                        Text = "git init",
                        IsCorrect = true,
                        QuestionId = 32
                    },
                    new QuestionOption
                    {
                        Text = "git start",
                        IsCorrect = false,
                        QuestionId = 32
                    },
                    new QuestionOption
                    {
                        Text = "git create",
                        IsCorrect = false,
                        QuestionId = 32
                    },
                    // Question 33
                    new QuestionOption
                    {
                        Text = "git commit",
                        IsCorrect = true,
                        QuestionId = 33
                    },
                    new QuestionOption
                    {
                        Text = "git push",
                        IsCorrect = false,
                        QuestionId = 33
                    },
                    new QuestionOption
                    {
                        Text = "git clone",
                        IsCorrect = false,
                        QuestionId = 33
                    },
                    // Question 34
                    new QuestionOption
                    {
                        Text = "git clone",
                        IsCorrect = true,
                        QuestionId = 34
                    },
                    new QuestionOption
                    {
                        Text = "git branch",
                        IsCorrect = false,
                        QuestionId = 34
                    },
                    new QuestionOption
                    {
                        Text = "git status",
                        IsCorrect = false,
                        QuestionId = 34
                    },
                    // Question 35
                    new QuestionOption
                    {
                        Text = "Multi-Factor Authentication",
                        IsCorrect = true,
                        QuestionId = 35
                    },
                    new QuestionOption
                    {
                        Text = "Main File Access",
                        IsCorrect = false,
                        QuestionId = 35
                    },
                    new QuestionOption
                    {
                        Text = "Multiple Firewall Application",
                        IsCorrect = false,
                        QuestionId = 35
                    },
                    // Question 36
                    new QuestionOption
                    {
                        Text = "A long unique password",
                        IsCorrect = true,
                        QuestionId = 36
                    },
                    new QuestionOption
                    {
                        Text = "password123",
                        IsCorrect = false,
                        QuestionId = 36
                    },
                    new QuestionOption
                    {
                        Text = "Your first name",
                        IsCorrect = false,
                        QuestionId = 36
                    },
                    // Question 37
                    new QuestionOption
                    {
                        Text = "Data sent between browser and server",
                        IsCorrect = true,
                        QuestionId = 37
                    },
                    new QuestionOption
                    {
                        Text = "The monitor",
                        IsCorrect = false,
                        QuestionId = 37
                    },
                    new QuestionOption
                    {
                        Text = "The keyboard",
                        IsCorrect = false,
                        QuestionId = 37
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
                    // Alice - HTML Attempt 1
                    // Score: 10 / 30
                    CreateAttemptAnswer(1, 1, 1),
                    CreateAttemptAnswer(1, 2, 5),
                    CreateAttemptAnswer(1, 3, 8),

                    // Alice - HTML Attempt 2
                    // Score: 20 / 30
                    CreateAttemptAnswer(2, 1, 1),
                    CreateAttemptAnswer(2, 2, 4),
                    CreateAttemptAnswer(2, 3, 8),

                    // Alice - HTML Attempt 3
                    // Score: 30 / 30
                    CreateAttemptAnswer(3, 1, 1),
                    CreateAttemptAnswer(3, 2, 4),
                    CreateAttemptAnswer(3, 3, 7),

                    // Bob - HTML
                    // Score: 30 / 30
                    CreateAttemptAnswer(4, 1, 1),
                    CreateAttemptAnswer(4, 2, 4),
                    CreateAttemptAnswer(4, 3, 7),

                    // Charlie - HTML
                    // Score: 20 / 30
                    CreateAttemptAnswer(5, 1, 1),
                    CreateAttemptAnswer(5, 2, 4),
                    CreateAttemptAnswer(5, 3, 8),

                    // Emma - HTML
                    // Score: 10 / 30
                    CreateAttemptAnswer(6, 1, 1),
                    CreateAttemptAnswer(6, 2, 5),
                    CreateAttemptAnswer(6, 3, 8),

                    // David - HTML
                    // Score: 30 / 30
                    CreateAttemptAnswer(7, 1, 1),
                    CreateAttemptAnswer(7, 2, 4),
                    CreateAttemptAnswer(7, 3, 7),

                    // Alice - C# first attempt
                    // Score: 30 / 40
                    CreateAttemptAnswer(14, 10, 28),
                    CreateAttemptAnswer(14, 11, 31),
                    CreateAttemptAnswer(14, 12, 34),
                    CreateAttemptAnswer(14, 13, 38),

                    // Alice - C# second attempt
                    // Score: 40 / 40
                    CreateAttemptAnswer(15, 10, 28),
                    CreateAttemptAnswer(15, 11, 31),
                    CreateAttemptAnswer(15, 12, 34),
                    CreateAttemptAnswer(15, 13, 37),

                    // Bob - C#
                    // Score: 40 / 40
                    CreateAttemptAnswer(16, 10, 28),
                    CreateAttemptAnswer(16, 11, 31),
                    CreateAttemptAnswer(16, 12, 34),
                    CreateAttemptAnswer(16, 13, 37)
                };

            context.AttemptAnswers.AddRange(
                attemptAnswers);

            context.SaveChanges();

            // Adds realistic activity for achievement testing.
            // Achievements themselves are still unlocked only
            // by AchievementService.
            await SeedUserActivityAsync(context, userManager);
        }

        private static void ClearUploadedImages()
        {
            string uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "images");

            if (!Directory.Exists(uploadsPath))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(uploadsPath))
            {
                File.Delete(filePath);
            }
        }

        private static async Task SeedUserActivityAsync(
            ChallengeDbContext context,
            UserManager<User> userManager)
        {
            var extraUsers = new List<User>
            {
                new User { Id = 7, UserName = "Freya", Email = "freya@example.com" },
                new User { Id = 8, UserName = "George", Email = "george@example.com" },
                new User { Id = 9, UserName = "Hannah", Email = "hannah@example.com" },
                new User { Id = 10, UserName = "Isaac", Email = "isaac@example.com" },
                new User { Id = 11, UserName = "Julia", Email = "julia@example.com" },
                new User { Id = 12, UserName = "Kevin", Email = "kevin@example.com" }
            };

            foreach (var user in extraUsers)
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create {user.UserName}: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
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
                    new QuestionOption { Text = item.Correct, IsCorrect = true, QuestionId = question.QuestionId },
                    new QuestionOption { Text = item.Incorrect1, IsCorrect = false, QuestionId = question.QuestionId },
                    new QuestionOption { Text = item.Incorrect2, IsCorrect = false, QuestionId = question.QuestionId });
            }

            context.SaveChanges();

            var now = DateTime.Now;
            var aliceExistingChallengeIds = new HashSet<int> { 1, 2, 3, 4, 5 };
            var aliceNewUserChallenges = new List<UserChallenge>();

            for (int challengeId = 6; challengeId <= 20; challengeId++)
            {
                if (aliceExistingChallengeIds.Contains(challengeId))
                {
                    continue;
                }

                aliceNewUserChallenges.Add(
                    new UserChallenge
                    {
                        UserId = 2,
                        ChallengeId = challengeId
                    });
            }

            context.UserChallenges.AddRange(aliceNewUserChallenges);
            context.SaveChanges();

            int dayOffset = 30;
            foreach (var userChallenge in aliceNewUserChallenges)
            {
                int maxPoints = context.Questions
                    .Where(q => q.ChallengeId == userChallenge.ChallengeId)
                    .Sum(q => q.Points);

                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = userChallenge.UserChallengeId,
                        Score = maxPoints,
                        Completed = true,
                        StartedAt = now.AddDays(-dayOffset).AddMinutes(-5),
                        CompletedAt = now.AddDays(-dayOffset)
                    });

                dayOffset--;
            }
            context.ChallengeAttempts.AddRange(
                new ChallengeAttempt
                {
                    UserChallengeId = 1,
                    Score = 20,
                    Completed = true,
                    StartedAt = now.AddHours(-8).AddMinutes(-5),
                    CompletedAt = now.AddHours(-8)
                },
                new ChallengeAttempt
                {
                    UserChallengeId = 1,
                    Score = 30,
                    Completed = true,
                    StartedAt = now.AddHours(-4).AddMinutes(-4),
                    CompletedAt = now.AddHours(-4)
                });

            context.SaveChanges();
            var participantIds = new List<int>
            {
                3, 4, 5, 6,
                extraUsers[0].Id,
                extraUsers[1].Id,
                extraUsers[2].Id,
                extraUsers[3].Id,
                extraUsers[4].Id,
                extraUsers[5].Id
            };

            int participantNumber = 0;
            foreach (int userId in participantIds)
            {
                var userChallenge = new UserChallenge
                {
                    UserId = userId,
                    ChallengeId = 13
                };

                context.UserChallenges.Add(userChallenge);
                context.SaveChanges();
                int score = participantNumber < 3 ? 10 : 0;
                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = userChallenge.UserChallengeId,
                        Score = score,
                        Completed = true,
                        StartedAt = now.AddDays(-2)
                            .AddMinutes(-(8 + participantNumber)),
                        CompletedAt = now.AddDays(-2)
                    });

                participantNumber++;
            }

            context.SaveChanges();
            foreach (int challengeId in new[] { 2, 5, 8, 19, 20 })
            {
                var userChallenge = new UserChallenge
                {
                    UserId = 3,
                    ChallengeId = challengeId
                };

                context.UserChallenges.Add(userChallenge);
                context.SaveChanges();

                int maxPoints = context.Questions
                    .Where(q => q.ChallengeId == challengeId)
                    .Sum(q => q.Points);

                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = userChallenge.UserChallengeId,
                        Score = maxPoints,
                        Completed = true,
                        StartedAt = now.AddDays(-12 - challengeId).AddMinutes(-7),
                        CompletedAt = now.AddDays(-12 - challengeId)
                    });
            }
            var bobWeb = context.UserChallenges
                .First(uc => uc.UserId == 3 && uc.ChallengeId == 3);

            for (int i = 0; i < 4; i++)
            {
                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = bobWeb.UserChallengeId,
                        Score = 30,
                        Completed = true,
                        StartedAt = now.AddDays(-1).AddHours(-(i + 1)).AddMinutes(-5),
                        CompletedAt = now.AddDays(-1).AddHours(-(i + 1))
                    });
            }
            foreach (int challengeId in new[] { 8, 9, 10, 14, 15 })
            {
                var userChallenge = new UserChallenge
                {
                    UserId = 6,
                    ChallengeId = challengeId
                };

                context.UserChallenges.Add(userChallenge);
                context.SaveChanges();

                int maxPoints = context.Questions
                    .Where(q => q.ChallengeId == challengeId)
                    .Sum(q => q.Points);

                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = userChallenge.UserChallengeId,
                        Score = maxPoints,
                        Completed = true,
                        StartedAt = now.AddDays(-18 - challengeId).AddMinutes(-8),
                        CompletedAt = now.AddDays(-18 - challengeId)
                    });
            }
            var davidCss = new UserChallenge
            {
                UserId = 6,
                ChallengeId = 6
            };

            context.UserChallenges.Add(davidCss);
            context.SaveChanges();
            context.ChallengeAttempts.Add(
                new ChallengeAttempt
                {
                    UserChallengeId = davidCss.UserChallengeId,
                    Score = 30,
                    Completed = true,
                    StartedAt = now.AddDays(-7).AddMinutes(-2),
                    CompletedAt = now.AddDays(-7)
                });

            context.SaveChanges();
            var charlieAccessibility = new UserChallenge
            {
                UserId = 4,
                ChallengeId = 14
            };

            context.UserChallenges.Add(charlieAccessibility);
            context.SaveChanges();
            context.ChallengeAttempts.Add(
                new ChallengeAttempt
                {
                    UserChallengeId = charlieAccessibility.UserChallengeId,
                    Score = 10,
                    Completed = true,
                    StartedAt = now.AddDays(-6).AddMinutes(-6),
                    CompletedAt = now.AddDays(-6)
                });

            context.SaveChanges();
            for (int i = 0; i < extraUsers.Count; i++)
            {
                int challengeId = 14 + (i % 3);
                var userChallenge = new UserChallenge
                {
                    UserId = extraUsers[i].Id,
                    ChallengeId = challengeId
                };

                context.UserChallenges.Add(userChallenge);
                context.SaveChanges();

                context.ChallengeAttempts.Add(
                    new ChallengeAttempt
                    {
                        UserChallengeId = userChallenge.UserChallengeId,
                        Score = i % 2 == 0 ? 10 : 0,
                        Completed = true,
                        StartedAt = now.AddDays(-(3 + i)).AddMinutes(-6),
                        CompletedAt = now.AddDays(-(3 + i))
                    });
            }

            context.SaveChanges();
            var furtherQuizzes = new (string Title, string Description, string Image,
                string Question, string Correct, string Incorrect1, string Incorrect2)[]
            {
                ("Java Fundamentals", "Explore Java syntax and object-oriented programming.", "/images/Java.png",
                    "Which keyword is used to inherit a class in Java?", "extends", "implements", "inherits"),
                ("Python Data Structures", "Practice lists, dictionaries and other Python collections.", "/images/Python.png",
                    "Which Python collection stores key-value pairs?", "dict", "list", "tuple"),
                ("SQL Joins", "Learn how to combine data from related database tables.", "/images/SQL.png",
                    "Which SQL join returns only matching rows from both tables?", "INNER JOIN", "LEFT JOIN", "FULL OUTER JOIN"),
                ("Git Branching", "Practice working with branches in version control.", "/images/GitHub.png",
                    "Which Git command creates a new branch without switching to it?", "git branch feature", "git merge feature", "git pull feature"),
                ("HTTP and REST", "Explore HTTP status codes and REST API conventions.", "/images/API.png",
                    "Which HTTP status code means a resource was not found?", "404", "200", "201")
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
                    ChallengeId = challenge.ChallengeId
                };
                context.Questions.Add(question);
                context.SaveChanges();

                context.QuestionOptions.AddRange(
                    new QuestionOption { Text = item.Correct, IsCorrect = true, QuestionId = question.QuestionId },
                    new QuestionOption { Text = item.Incorrect1, IsCorrect = false, QuestionId = question.QuestionId },
                    new QuestionOption { Text = item.Incorrect2, IsCorrect = false, QuestionId = question.QuestionId });
                context.SaveChanges();

                var aliceChallenge = new UserChallenge
                {
                    UserId = 2,
                    ChallengeId = challenge.ChallengeId
                };
                context.UserChallenges.Add(aliceChallenge);
                context.SaveChanges();

                context.ChallengeAttempts.Add(new ChallengeAttempt
                {
                    UserChallengeId = aliceChallenge.UserChallengeId,
                    Score = 10,
                    Completed = true,
                    StartedAt = now.AddDays(-(40 + Array.IndexOf(furtherQuizzes, item) + 1)).AddMinutes(-5),
                    CompletedAt = now.AddDays(-(40 + Array.IndexOf(furtherQuizzes, item) + 1))
                });
                context.SaveChanges();
            }
        }

        private static AttemptAnswer CreateAttemptAnswer(
            int challengeAttemptId,
            int questionId,
            params int[] selectedOptionIds)
        {
            var attemptAnswer =
                new AttemptAnswer
                {
                    ChallengeAttemptId =
                        challengeAttemptId,
                    QuestionId =
                        questionId
                };

            foreach (int optionId in selectedOptionIds)
            {
                attemptAnswer.SelectedOptions.Add(
                    new AttemptAnswerOption
                    {
                        QuestionOptionId =
                            optionId
                    });
            }

            return attemptAnswer;
        }
    }
}
