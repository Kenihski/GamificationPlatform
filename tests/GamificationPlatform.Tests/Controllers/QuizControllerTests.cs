using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class QuizControllerTests
{
    [Theory]
    [InlineData(0, 8)]
    [InlineData(10, 0)]
    [InlineData(-1, 8)]
    public async Task Submit_WhenIdsAreInvalid_ReturnsBadRequestWithoutSaving(
        int challengeId, int attemptId)
    {
        var attemptRepository = Substitute.For<IAttemptRepository>();
        var logger = Substitute.For<ILogger<QuizController>>();
        var controller = CreateController(
            Substitute.For<IChallengeRepository>(), attemptRepository, logger);

        var result = await controller.Submit(challengeId, attemptId, null, null);

        Assert.IsType<BadRequestResult>(result);
        await attemptRepository.DidNotReceive().SaveChangesAsync();
        AssertWarning(logger, "submitted fields were invalid");
    }

    [Fact]
    public async Task Submit_WhenModelStateIsInvalid_ReturnsBadRequestWithoutSaving()
    {
        var attemptRepository = Substitute.For<IAttemptRepository>();
        var logger = Substitute.For<ILogger<QuizController>>();
        var controller = CreateController(
            Substitute.For<IChallengeRepository>(), attemptRepository, logger);
        controller.ModelState.AddModelError("answers", "Malformed selection");

        var result = await controller.Submit(10, 8, null, null);

        Assert.IsType<BadRequestResult>(result);
        await attemptRepository.DidNotReceive().SaveChangesAsync();
        AssertWarning(logger, "submitted fields were invalid");
    }

    [Theory]
    [InlineData(999, 101, "invalid choice answer")]
    [InlineData(21, 999, "invalid option")]
    [InlineData(21, 102, "multiple options")]
    public async Task Submit_WhenAnswerDoesNotBelongToQuestion_RejectsWithoutSaving(
        int submittedQuestionId, int submittedOptionId, string warningText)
    {
        // Arrange: an existing, published question and unfinished attempt.
        var challengeRepository = Substitute.For<IChallengeRepository>();
        var attemptRepository = Substitute.For<IAttemptRepository>();
        var logger = Substitute.For<ILogger<QuizController>>();

        var challenge = new Challenge
        {
            ChallengeId = 10,
            IsPublished = true,
            CreatedByUserId = 99,
            Title = "Published challenge",
            Questions = new List<Question>
            {
                new Question
                {
                    QuestionId = 21,
                    ChallengeId = 10,
                    Points = 10,
                    QuestionType = "SingleChoice",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 101, Text = "A" },
                        new QuestionOption { QuestionOptionId = 102, Text = "B" }
                    }
                }
            }
        };

        challengeRepository.GetChallengeWithQuestionsAndOptionsAsync(10)
            .Returns(Task.FromResult<Challenge?>(challenge));

        attemptRepository.GetValidUnfinishedAttemptAsync(8, 7, 10)
            .Returns(Task.FromResult<ChallengeAttempt?>(new ChallengeAttempt
            {
                ChallengeAttemptId = 8,
                StartedAt = DateTime.Now
            }));

        var controller = CreateController(
            challengeRepository, attemptRepository, logger);

        var selected = submittedOptionId == 102
            ? new List<int> { 101, 102 }
            : new List<int> { submittedOptionId };
        var answers = new Dictionary<int, List<int>>
        {
            [submittedQuestionId] = selected
        };

        // Act
        var result = await controller.Submit(10, 8, answers, null);

        // Assert: input rejected before persisting an attempt.
        Assert.IsType<BadRequestResult>(result);
        await attemptRepository.DidNotReceive().SaveChangesAsync();
        AssertWarning(logger, warningText);
    }

    private static QuizController CreateController(
        IChallengeRepository challengeRepository,
        IAttemptRepository attemptRepository,
        ILogger<QuizController> logger)
    {
        var controller = new QuizController(
            challengeRepository,
            attemptRepository,
            Substitute.For<IUserRepository>(),
            Substitute.For<IAchievementService>(),
            logger);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "7") },
                    "Test"))
            }
        };
        return controller;
    }

    private static void AssertWarning(
        ILogger<QuizController> logger, string text)
    {
        Assert.Contains(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Warning) &&
            (call.GetArguments()[2]?.ToString() ?? "").Contains(text));
    }
}
