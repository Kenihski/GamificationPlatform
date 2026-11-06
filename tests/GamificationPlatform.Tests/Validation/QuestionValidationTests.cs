using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Validation;

public class QuestionValidationTests
{
    [Theory]
    [InlineData("SingleChoice", "none")]
    [InlineData("SingleChoice", "line-endings")]
    [InlineData("MultipleChoice", "none")]
    [InlineData("ShortAnswer", "none")]
    [InlineData("SingleChoice", "title")]
    [InlineData("SingleChoice", "description")]
    [InlineData("SingleChoice", "explanation")]
    [InlineData("SingleChoice", "points")]
    [InlineData("SingleChoice", "image")]
    [InlineData("SingleChoice", "type")]
    [InlineData("SingleChoice", "answer")]
    [InlineData("SingleChoice", "correct")]
    [InlineData("ShortAnswer", "answer")]
    public async Task Update_OnlyResetsHistoryWhenQuestionChanges(string type, string change)
    {
        // Arrange: a valid existing question and the values returned by its edit form.
        var questions = Substitute.For<IQuestionRepository>();
        var attempts = Substitute.For<IAttemptRepository>();
        var images = Substitute.For<IImageService>();
        images.IsAllowedImageAsync(Arg.Any<string>(), 7).Returns(true);
        var stored = new Question
        {
            QuestionId = 1, ChallengeId = 12, Title = "Example", Points = 1,
            QuestionType = type, ImageUrl = "",
            Challenge = new Challenge { ChallengeId = 12, CreatedByUserId = 7 }
        };
        if (type == "ShortAnswer")
            stored.AcceptedAnswers.Add(new QuestionAcceptedAnswer { Text = "Yes" });
        else
            stored.Options.AddRange(new[]
            {
                new QuestionOption { Text = "Yes", IsCorrect = true },
                new QuestionOption { Text = "No", IsCorrect = false }
            });
        questions.GetQuestionWithOptionsAndChallengeAsync(1).Returns(stored);
        var controller = new QuestionController(questions, Substitute.For<IChallengeRepository>(),
            attempts, images, Substitute.For<ILogger<QuestionController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "7") }, "Test"))
            }
        };
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            controller.HttpContext, Substitute.For<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        var model = new QuestionFormViewModel
        {
            Question = new Question
            {
                QuestionId = 1, Title = "Example", Points = 1, QuestionType = type,
                Description = "", Explanation = "", ImageUrl = null
            },
            Options = new List<string> { " Yes ", "No", "", "" },
            CorrectOptions = new List<int> { 0 },
            AcceptedAnswers = new List<string> { " Yes ", "", "", "" }
        };
        switch (change)
        {
            case "line-endings":
                stored.Explanation = "Line one\nLine two";
                model.Question.Explanation = "Line one\r\nLine two";
                break;
            case "title": model.Question.Title = "Changed"; break;
            case "description": model.Question.Description = "Changed"; break;
            case "explanation": model.Question.Explanation = "Changed"; break;
            case "points": model.Question.Points = 2; break;
            case "image": model.Question.ImageUrl = "/images/Question_1.png"; break;
            case "type": model.Question.QuestionType = "MultipleChoice"; break;
            case "answer":
                model.Options[0] = "Maybe";
                model.AcceptedAnswers[0] = "Maybe";
                break;
            case "correct": model.CorrectOptions = new List<int> { 1 }; break;
        }

        // Act: submit without an upload.
        var result = await controller.Update(model, null);

        // Assert: unchanged forms must not write data or delete history.
        Assert.IsType<RedirectToActionResult>(result);
        int expectedWrites = change == "none" || change == "line-endings" ? 0 : 1;
        await attempts.Received(expectedWrites).DeleteChallengeHistoryAsync(12);
        await questions.Received(expectedWrites).UpdateQuestionAsync(Arg.Any<Question>());
        if (expectedWrites == 0)
            Assert.Equal("No changes were made.", controller.TempData["SuccessMessage"]);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void PointsValidation_WhenValueChanges_ReturnsExpectedResult(
        int points, bool expectedValid)
    {
        // Arrange: use the validation attribute on the actual model.
        var question = new Question { Points = points };
        var context = new ValidationContext(question)
        {
            MemberName = nameof(Question.Points)
        };
        var errors = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateProperty(
            question.Points, context, errors);

        // Assert
        Assert.Equal(expectedValid, valid);
    }

    [Theory]
    [InlineData("too-few-options", "Options")]
    [InlineData("duplicate-options", "Options")]
    [InlineData("too-many-options", "Options")]
    [InlineData("long-option", "Options")]
    [InlineData("no-correct-option", "CorrectOptions")]
    [InlineData("wrong-correct-index", "CorrectOptions")]
    [InlineData("multiple-single-choice-answers", "CorrectOptions")]
    [InlineData("duplicate-short-answers", "AcceptedAnswers")]
    [InlineData("missing-short-answers", "AcceptedAnswers")]
    [InlineData("invalid-question-type", "Question.QuestionType")]
    public async Task Create_WhenAnswersAreInvalid_ReturnsFormWithoutSavingAndLogsWarning(
        string scenario, string expectedErrorField)
    {
        // Arrange: a signed-in owner is creating a question in their own challenge.
        var questionRepository = Substitute.For<IQuestionRepository>();
        var challengeRepository = Substitute.For<IChallengeRepository>();
        var attemptRepository = Substitute.For<IAttemptRepository>();
        var imageService = Substitute.For<IImageService>();
        var logger = Substitute.For<ILogger<QuestionController>>();

        const int userId = 7;
        const int challengeId = 12;

        challengeRepository.GetChallengeByIdAsync(challengeId)
            .Returns(Task.FromResult<Challenge?>(new Challenge
            {
                ChallengeId = challengeId,
                CreatedByUserId = userId
            }));
        challengeRepository.GetChallengesByUserIdAsync(userId)
            .Returns(Task.FromResult(new List<Challenge>()));
        // New questions have a null ImageUrl when no image is selected.
        imageService.IsAllowedImageAsync(null, userId)
            .Returns(Task.FromResult(true));
        imageService.GetUserImagesAsync(userId)
            .Returns(Task.FromResult(new List<UserImage>()));
        imageService.GetDefaultImagesAsync()
            .Returns(Task.FromResult(new List<DefaultImage>()));

        var controller = new QuestionController(
            questionRepository, challengeRepository, attemptRepository,
            imageService, logger);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                    "Test"))
            }
        };

        var model = new QuestionFormViewModel
        {
            Question = new Question
            {
                ChallengeId = challengeId,
                Title = "Example question",
                Points = 10,
                QuestionType = "SingleChoice"
            },
            Options = new List<string> { "Yes", "No", "", "" },
            CorrectOptions = new List<int> { 0 }
        };

        switch (scenario)
        {
            case "too-few-options":
                model.Options = new List<string> { "Yes", "", "", "" };
                break;
            case "duplicate-options":
                model.Options = new List<string> { "Yes", " yes ", "", "" };
                break;
            case "too-many-options":
                model.Options = new List<string> { "A", "B", "C", "D", "E" };
                break;
            case "long-option":
                model.Options[0] = new string('A', 201);
                break;
            case "no-correct-option":
                model.CorrectOptions.Clear();
                break;
            case "wrong-correct-index":
                model.CorrectOptions = new List<int> { 3 };
                break;
            case "multiple-single-choice-answers":
                model.CorrectOptions = new List<int> { 0, 1 };
                break;
            case "duplicate-short-answers":
                model.Question.QuestionType = "ShortAnswer";
                model.AcceptedAnswers = new List<string> { "Norway", " norway ", "", "" };
                break;
            case "missing-short-answers":
                model.Question.QuestionType = "ShortAnswer";
                model.AcceptedAnswers = new List<string> { "", "", "", "" };
                break;
            case "invalid-question-type":
                model.Question.QuestionType = "Unknown";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        // Act: controller's custom answer validation runs even in a direct unit call.
        var result = await controller.Create(model, imageUpload: null);

        // Assert: an invalid question cannot erase attempts or be created.
        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState, entry =>
            entry.Key == expectedErrorField &&
            entry.Value?.Errors.Count > 0);

        // Image validation must not create an unrelated error.
        Assert.DoesNotContain(controller.ModelState, entry =>
            entry.Key == "Question.ImageUrl" &&
            entry.Value?.Errors.Count > 0);

        await questionRepository.DidNotReceive()
            .CreateQuestionAsync(Arg.Any<Question>());
        await attemptRepository.DidNotReceive()
            .DeleteChallengeHistoryAsync(Arg.Any<int>());

        Assert.Contains(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Warning) &&
            (call.GetArguments()[2]?.ToString() ?? "")
                .Contains("validation failed"));
    }
}
