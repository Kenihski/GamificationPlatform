using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class ChallengeControllerTests
{
    [Fact]
    public async Task Create_WhenModelStateIsInvalid_ReturnsFormWithoutSavingAndLogsWarning()
    {
        // Arrange: substitute the controller's dependencies.
        var challengeRepository =
            Substitute.For<IChallengeRepository>();

        var attemptRepository =
            Substitute.For<IAttemptRepository>();

        var userImageRepository =
            Substitute.For<IUserImageRepository>();

        var environment =
            Substitute.For<IWebHostEnvironment>();

        var achievementService =
            Substitute.For<IAchievementService>();

        var logger =
            Substitute.For<ILogger<ChallengeController>>();

        const int userId = 7;

        userImageRepository
            .GetImagesByUserIdAsync(userId)
            .Returns(Task.FromResult(new List<UserImage>()));

        // Substitute the default-image query with an empty collection.
        // No database provider or connection is configured.
        var options =
            new DbContextOptionsBuilder<ChallengeDbContext>().Options;

        using var context = new ChallengeDbContext(options)
        {
            DefaultImages = new List<DefaultImage>().BuildMockDbSet()
        };

        var controller = new ChallengeController(
            challengeRepository,
            attemptRepository,
            userImageRepository,
            context,
            environment,
            achievementService,
            logger);

        // Supply the signed-in user read by the controller.
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString())
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var challenge = new Challenge
        {
            Title = "",
            Description = "A challenge submitted without a title.",
            ImageUrl = ""
        };

        // Direct controller calls do not run MVC's validation pipeline.
        // Simulate the validation error MVC would supply.
        controller.ModelState.AddModelError(
            nameof(Challenge.Title),
            "Challenge title is required.");

        // Act: submit the challenge without an uploaded image.
        var result = await controller.Create(
            challenge,
            imageUpload: null);

        // Assert: return the submitted model and retain its error.
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(challenge, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);

        Assert.Contains(
            controller.ModelState[nameof(Challenge.Title)]!.Errors,
            error => error.ErrorMessage == "Challenge title is required.");

        // Invalid input must not reach the create operation.
        await challengeRepository
            .DidNotReceive()
            .CreateChallengeAsync(Arg.Any<Challenge>());

        // LogWarning is an extension method that calls ILogger.Log.
        // Inspect that recorded call to verify the warning.
        var warning = Assert.Single(
        logger.ReceivedCalls(),
        call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Warning));

        Assert.Contains(
            "validation failed",
            warning.GetArguments()[2]?.ToString() ?? "");
    }
}