using Microsoft.AspNetCore.Identity;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public static class QuestionSeeder
    {
        public static void Seed(ChallengeDbContext context)
        {
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

            // Additional questions (original question IDs 1-37 remain unchanged).
            var additionalQuestions = new List<Question>
            {
                new Question { ChallengeId = 1, Title = "Which HTML element contains the main navigation links?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "<footer>", IsCorrect = false }, new QuestionOption { Text = "<nav>", IsCorrect = true }, new QuestionOption { Text = "<aside>", IsCorrect = false }, new QuestionOption { Text = "<section>", IsCorrect = false } } },
                new Question { ChallengeId = 1, Title = "Which HTML elements are semantic landmarks?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "<main>", IsCorrect = true }, new QuestionOption { Text = "<header>", IsCorrect = true }, new QuestionOption { Text = "<span>", IsCorrect = false }, new QuestionOption { Text = "<footer>", IsCorrect = true } } },
                new Question { ChallengeId = 1, Title = "Which HTML attribute provides alternative text for an image?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "alt" } } },
                new Question { ChallengeId = 1, Title = "Which input type hides characters entered in a form?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "text", IsCorrect = false }, new QuestionOption { Text = "email", IsCorrect = false }, new QuestionOption { Text = "password", IsCorrect = true }, new QuestionOption { Text = "hidden", IsCorrect = false } } },
                new Question { ChallengeId = 1, Title = "Which tag creates a numbered list?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "<ul>", IsCorrect = false }, new QuestionOption { Text = "<dl>", IsCorrect = false }, new QuestionOption { Text = "<li>", IsCorrect = false }, new QuestionOption { Text = "<ol>", IsCorrect = true } } },
                new Question { ChallengeId = 2, Title = "What does a function return when its declared return type is void?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "A string", IsCorrect = false }, new QuestionOption { Text = "No value", IsCorrect = true }, new QuestionOption { Text = "A boolean", IsCorrect = false }, new QuestionOption { Text = "An integer", IsCorrect = false } } },
                new Question { ChallengeId = 2, Title = "Which are common control-flow structures?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "if statement", IsCorrect = true }, new QuestionOption { Text = "for loop", IsCorrect = true }, new QuestionOption { Text = "Class declaration", IsCorrect = false }, new QuestionOption { Text = "while loop", IsCorrect = true } } },
                new Question { ChallengeId = 2, Title = "What is the common name for an error in program logic?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "bug" } } },
                new Question { ChallengeId = 2, Title = "Which data structure uses first-in, first-out ordering?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Stack", IsCorrect = false }, new QuestionOption { Text = "Queue", IsCorrect = true }, new QuestionOption { Text = "Tree", IsCorrect = false }, new QuestionOption { Text = "Set", IsCorrect = false } } },
                new Question { ChallengeId = 2, Title = "What is the result of 7 % 3 in many programming languages?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "0", IsCorrect = false }, new QuestionOption { Text = "2", IsCorrect = false }, new QuestionOption { Text = "1", IsCorrect = true }, new QuestionOption { Text = "3", IsCorrect = false } } },
                new Question { ChallengeId = 3, Title = "Which HTTP method is typically used to retrieve a resource?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "POST", IsCorrect = false }, new QuestionOption { Text = "PUT", IsCorrect = false }, new QuestionOption { Text = "GET", IsCorrect = true }, new QuestionOption { Text = "DELETE", IsCorrect = false } } },
                new Question { ChallengeId = 3, Title = "Which technologies can run in a web browser?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "HTML", IsCorrect = true }, new QuestionOption { Text = "CSS", IsCorrect = true }, new QuestionOption { Text = "JavaScript", IsCorrect = true }, new QuestionOption { Text = "SQL Server", IsCorrect = false } } },
                new Question { ChallengeId = 3, Title = "Which HTTP status code means Not Found?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "404" } } },
                new Question { ChallengeId = 3, Title = "What does an API primarily enable?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "File compression", IsCorrect = false }, new QuestionOption { Text = "Communication between software systems", IsCorrect = true }, new QuestionOption { Text = "Screen calibration", IsCorrect = false }, new QuestionOption { Text = "Database encryption", IsCorrect = false } } },
                new Question { ChallengeId = 3, Title = "Which HTTP status code indicates a successful request?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "200", IsCorrect = true }, new QuestionOption { Text = "301", IsCorrect = false }, new QuestionOption { Text = "404", IsCorrect = false }, new QuestionOption { Text = "500", IsCorrect = false } } },
                new Question { ChallengeId = 4, Title = "Which programming language is commonly used with .NET?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Java", IsCorrect = false }, new QuestionOption { Text = "Python", IsCorrect = false }, new QuestionOption { Text = "C#", IsCorrect = true }, new QuestionOption { Text = "Ruby", IsCorrect = false } } },
                new Question { ChallengeId = 4, Title = "Which are valid C# access modifiers?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "public", IsCorrect = true }, new QuestionOption { Text = "private", IsCorrect = true }, new QuestionOption { Text = "protected", IsCorrect = true }, new QuestionOption { Text = "external", IsCorrect = false } } },
                new Question { ChallengeId = 4, Title = "Which keyword awaits an asynchronous operation in C#?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "await" } } },
                new Question { ChallengeId = 4, Title = "Which interface supports iterating over a collection in C#?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "IDisposable", IsCorrect = false }, new QuestionOption { Text = "IComparable", IsCorrect = false }, new QuestionOption { Text = "IEnumerable", IsCorrect = true }, new QuestionOption { Text = "ICloneable", IsCorrect = false } } },
                new Question { ChallengeId = 5, Title = "What uniquely identifies a row in a relational table?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Foreign key", IsCorrect = false }, new QuestionOption { Text = "Primary key", IsCorrect = true }, new QuestionOption { Text = "Index name", IsCorrect = false }, new QuestionOption { Text = "Table alias", IsCorrect = false } } },
                new Question { ChallengeId = 5, Title = "Which statements about foreign keys are correct?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "They can reference another table", IsCorrect = true }, new QuestionOption { Text = "They help enforce relationships", IsCorrect = true }, new QuestionOption { Text = "They must always be unique", IsCorrect = false }, new QuestionOption { Text = "They can help maintain referential integrity", IsCorrect = true } } },
                new Question { ChallengeId = 5, Title = "What does SQL stand for?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "Structured Query Language" } } },
                new Question { ChallengeId = 5, Title = "Which JOIN returns only matching rows from both tables?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "LEFT JOIN", IsCorrect = false }, new QuestionOption { Text = "RIGHT JOIN", IsCorrect = false }, new QuestionOption { Text = "FULL JOIN", IsCorrect = false }, new QuestionOption { Text = "INNER JOIN", IsCorrect = true } } },
                new Question { ChallengeId = 5, Title = "What is a common goal of database normalization?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Duplicate all records", IsCorrect = false }, new QuestionOption { Text = "Reduce redundant data", IsCorrect = true }, new QuestionOption { Text = "Remove all indexes", IsCorrect = false }, new QuestionOption { Text = "Avoid primary keys", IsCorrect = false } } },
                new Question { ChallengeId = 6, Title = "Which CSS property changes the text color?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "background-color", IsCorrect = false }, new QuestionOption { Text = "font-size", IsCorrect = false }, new QuestionOption { Text = "color", IsCorrect = true }, new QuestionOption { Text = "opacity", IsCorrect = false } } },
                new Question { ChallengeId = 6, Title = "Which values are valid for CSS display?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "grid", IsCorrect = true }, new QuestionOption { Text = "flex", IsCorrect = true }, new QuestionOption { Text = "block", IsCorrect = true }, new QuestionOption { Text = "centered", IsCorrect = false } } },
                new Question { ChallengeId = 6, Title = "Which CSS property controls space inside the border of an element?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "padding" } } },
                new Question { ChallengeId = 6, Title = "Which selector targets an element with id=\"menu\"?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = " .menu", IsCorrect = false }, new QuestionOption { Text = "element.menu", IsCorrect = false }, new QuestionOption { Text = "#menu", IsCorrect = true }, new QuestionOption { Text = "element.menu", IsCorrect = false } } },
                new Question { ChallengeId = 6, Title = "Which layout system is designed for two-dimensional layouts?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Flexbox", IsCorrect = false }, new QuestionOption { Text = "CSS Grid", IsCorrect = true }, new QuestionOption { Text = "Float", IsCorrect = false }, new QuestionOption { Text = "Inline", IsCorrect = false } } },
                new Question { ChallengeId = 7, Title = "In MVC, which component handles incoming requests?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Model", IsCorrect = false }, new QuestionOption { Text = "View", IsCorrect = false }, new QuestionOption { Text = "Controller", IsCorrect = true }, new QuestionOption { Text = "Migration", IsCorrect = false } } },
                new Question { ChallengeId = 7, Title = "Which are built-in ASP.NET Core concepts?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Middleware", IsCorrect = true }, new QuestionOption { Text = "Dependency injection", IsCorrect = true }, new QuestionOption { Text = "Routing", IsCorrect = true }, new QuestionOption { Text = "Browser DOM", IsCorrect = false } } },
                new Question { ChallengeId = 7, Title = "Which EF Core method saves tracked changes asynchronously?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "SaveChangesAsync" } } },
                new Question { ChallengeId = 7, Title = "What does dependency injection primarily help with?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Image rendering", IsCorrect = false }, new QuestionOption { Text = "Managing dependencies between components", IsCorrect = true }, new QuestionOption { Text = "Encrypting every database field", IsCorrect = false }, new QuestionOption { Text = "Replacing HTTP requests", IsCorrect = false } } },
                new Question { ChallengeId = 7, Title = "Which Razor syntax starts a C# expression in a view?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "%", IsCorrect = false }, new QuestionOption { Text = "@", IsCorrect = true }, new QuestionOption { Text = "&", IsCorrect = false }, new QuestionOption { Text = "#", IsCorrect = false } } },
                new Question { ChallengeId = 8, Title = "Which keyword declares a block-scoped variable that can be reassigned?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "var", IsCorrect = false }, new QuestionOption { Text = "let", IsCorrect = true }, new QuestionOption { Text = "const", IsCorrect = false }, new QuestionOption { Text = "static", IsCorrect = false } } },
                new Question { ChallengeId = 8, Title = "Which are JavaScript primitive types?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "string", IsCorrect = true }, new QuestionOption { Text = "number", IsCorrect = true }, new QuestionOption { Text = "boolean", IsCorrect = true }, new QuestionOption { Text = "array", IsCorrect = false } } },
                new Question { ChallengeId = 8, Title = "Which keyword declares an asynchronous JavaScript function?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "async" } } },
                new Question { ChallengeId = 8, Title = "Which method selects the first matching element using a CSS selector?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "getElementById", IsCorrect = false }, new QuestionOption { Text = "querySelector", IsCorrect = true }, new QuestionOption { Text = "querySelectorAll", IsCorrect = false }, new QuestionOption { Text = "createElement", IsCorrect = false } } },
                new Question { ChallengeId = 8, Title = "What does a Promise represent?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "A CSS rule", IsCorrect = false }, new QuestionOption { Text = "A database table", IsCorrect = false }, new QuestionOption { Text = "An eventual asynchronous result", IsCorrect = true }, new QuestionOption { Text = "A fixed loop count", IsCorrect = false } } },
                new Question { ChallengeId = 9, Title = "Which Python collection is mutable and ordered?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "tuple", IsCorrect = false }, new QuestionOption { Text = "list", IsCorrect = true }, new QuestionOption { Text = "frozenset", IsCorrect = false }, new QuestionOption { Text = "str", IsCorrect = false } } },
                new Question { ChallengeId = 9, Title = "Which are valid Python built-in collection types?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "list", IsCorrect = true }, new QuestionOption { Text = "dict", IsCorrect = true }, new QuestionOption { Text = "set", IsCorrect = true }, new QuestionOption { Text = "matrix", IsCorrect = false } } },
                new Question { ChallengeId = 9, Title = "Which Python keyword defines a function?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "def" } } },
                new Question { ChallengeId = 9, Title = "What is the result of len([1, 2, 3])?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "2", IsCorrect = false }, new QuestionOption { Text = "3", IsCorrect = true }, new QuestionOption { Text = "4", IsCorrect = false }, new QuestionOption { Text = "6", IsCorrect = false } } },
                new Question { ChallengeId = 9, Title = "Which operator tests equality in Python?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "=", IsCorrect = false }, new QuestionOption { Text = "!=", IsCorrect = false }, new QuestionOption { Text = "==", IsCorrect = true }, new QuestionOption { Text = "is not", IsCorrect = false } } },
                new Question { ChallengeId = 10, Title = "Which SQL clause filters rows before grouping?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "ORDER BY", IsCorrect = false }, new QuestionOption { Text = "WHERE", IsCorrect = true }, new QuestionOption { Text = "HAVING", IsCorrect = false }, new QuestionOption { Text = "GROUP BY", IsCorrect = false } } },
                new Question { ChallengeId = 10, Title = "Which are SQL aggregate functions?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "COUNT", IsCorrect = true }, new QuestionOption { Text = "SUM", IsCorrect = true }, new QuestionOption { Text = "AVG", IsCorrect = true }, new QuestionOption { Text = "DISTINCT", IsCorrect = false } } },
                new Question { ChallengeId = 10, Title = "Which SQL keyword sorts query results?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "ORDER BY" } } },
                new Question { ChallengeId = 10, Title = "Which SQL clause filters groups after aggregation?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "WHERE", IsCorrect = false }, new QuestionOption { Text = "JOIN", IsCorrect = false }, new QuestionOption { Text = "HAVING", IsCorrect = true }, new QuestionOption { Text = "LIMIT", IsCorrect = false } } },
                new Question { ChallengeId = 10, Title = "Which query counts rows in a table named Users?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "SELECT SUM(*) FROM Users", IsCorrect = false }, new QuestionOption { Text = "SELECT COUNT(*) FROM Users", IsCorrect = true }, new QuestionOption { Text = "SELECT TOTAL(*) FROM Users", IsCorrect = false }, new QuestionOption { Text = "SELECT SIZE(*) FROM Users", IsCorrect = false } } },
                new Question { ChallengeId = 11, Title = "Which Git command records staged changes as a commit?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "git push", IsCorrect = false }, new QuestionOption { Text = "git add", IsCorrect = false }, new QuestionOption { Text = "git commit", IsCorrect = true }, new QuestionOption { Text = "git fetch", IsCorrect = false } } },
                new Question { ChallengeId = 11, Title = "Which commands can inspect repository history or changes?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "git log", IsCorrect = true }, new QuestionOption { Text = "git diff", IsCorrect = true }, new QuestionOption { Text = "git status", IsCorrect = true }, new QuestionOption { Text = "git init", IsCorrect = false } } },
                new Question { ChallengeId = 11, Title = "Which Git command creates a new branch named feature?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "git branch feature" } } },
                new Question { ChallengeId = 11, Title = "Which command downloads remote changes without merging them?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "git pull", IsCorrect = false }, new QuestionOption { Text = "git fetch", IsCorrect = true }, new QuestionOption { Text = "git merge", IsCorrect = false }, new QuestionOption { Text = "git commit", IsCorrect = false } } },
                new Question { ChallengeId = 11, Title = "What is a merge conflict?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "A successful push", IsCorrect = false }, new QuestionOption { Text = "Incompatible changes that need resolution", IsCorrect = true }, new QuestionOption { Text = "A deleted remote", IsCorrect = false }, new QuestionOption { Text = "An empty commit", IsCorrect = false } } },
                new Question { ChallengeId = 12, Title = "What does the principle of least privilege mean?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Everyone gets admin rights", IsCorrect = false }, new QuestionOption { Text = "Users get only the access they need", IsCorrect = true }, new QuestionOption { Text = "Passwords are optional", IsCorrect = false }, new QuestionOption { Text = "All files are public", IsCorrect = false } } },
                new Question { ChallengeId = 12, Title = "Which practices help protect user accounts?", Description = "Choose the correct answer.", Points = 10, QuestionType = "MultipleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Multi-factor authentication", IsCorrect = true }, new QuestionOption { Text = "Unique passwords", IsCorrect = true }, new QuestionOption { Text = "Password hashing", IsCorrect = true }, new QuestionOption { Text = "Sharing login credentials", IsCorrect = false } } },
                new Question { ChallengeId = 12, Title = "What does XSS stand for in web security?", Description = "Choose the correct answer.", Points = 10, QuestionType = "ShortAnswer", AcceptedAnswers = new List<QuestionAcceptedAnswer> { new QuestionAcceptedAnswer { Text = "Cross-Site Scripting" } } },
                new Question { ChallengeId = 12, Title = "Which technique helps prevent SQL injection?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "String concatenation", IsCorrect = false }, new QuestionOption { Text = "Parameterized queries", IsCorrect = true }, new QuestionOption { Text = "Disabling HTTPS", IsCorrect = false }, new QuestionOption { Text = "Client-side validation alone", IsCorrect = false } } },
                new Question { ChallengeId = 12, Title = "What is the main purpose of TLS for web traffic?", Description = "Choose the correct answer.", Points = 10, QuestionType = "SingleChoice", Options = new List<QuestionOption> { new QuestionOption { Text = "Compress images", IsCorrect = false }, new QuestionOption { Text = "Protect data in transit", IsCorrect = true }, new QuestionOption { Text = "Store passwords", IsCorrect = false }, new QuestionOption { Text = "Block all malware", IsCorrect = false } } },
            };
            context.Questions.AddRange(additionalQuestions);
            context.SaveChanges();

            // Recalculate maximum scores after adding the expanded question bank.
            foreach (var challenge in context.Challenges.ToList())
            {
                challenge.MaxPoints = context.Questions
                    .Where(q => q.ChallengeId == challenge.ChallengeId)
                    .Sum(q => q.Points);
            }
            context.SaveChanges();
            // --------------------------------
        }
    }
}
