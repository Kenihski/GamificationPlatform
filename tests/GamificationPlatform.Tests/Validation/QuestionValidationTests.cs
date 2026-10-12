using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Validation;

public class QuestionValidationTests
{
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
        var imageRepository = Substitute.For<IUserImageRepository>();
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
        imageRepository.GetImagesByUserIdAsync(userId)
            .Returns(Task.FromResult(new List<UserImage>()));

        using var context = new ChallengeDbContext(
            new DbContextOptionsBuilder<ChallengeDbContext>().Options)
        {
            DefaultImages = new List<DefaultImage>().BuildMockDbSet()
        };

        var controller = new QuestionController(
            questionRepository, challengeRepository, attemptRepository,
            imageRepository, context, Substitute.For<IWebHostEnvironment>(),
            logger);

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
