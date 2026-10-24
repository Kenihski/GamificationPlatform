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

        var imageService =
            Substitute.For<IImageService>();

        var achievementService =
            Substitute.For<IAchievementService>();

        var logger =
            Substitute.For<ILogger<ChallengeController>>();

        const int userId = 7;

        imageService
            .IsAllowedImageAsync(string.Empty, userId)
            .Returns(Task.FromResult(true));
        imageService
            .GetUserImagesAsync(userId)
            .Returns(Task.FromResult(new List<UserImage>()));
        imageService
            .GetDefaultImagesAsync()
            .Returns(Task.FromResult(new List<DefaultImage>()));

        var controller = new ChallengeController(
            challengeRepository,
            attemptRepository,
            imageService,
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

    [Fact]
    public async Task Update_WhenChallengeDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        // Arrange
        var repository = Substitute.For<IChallengeRepository>();
        var logger = Substitute.For<ILogger<ChallengeController>>();

        repository.GetChallengeByIdAsync(99)
            .Returns(Task.FromResult<Challenge?>(null));

        var controller = CreateUpdateController(
            repository, logger, userId: 7);

        var submittedChallenge = new Challenge
        {
            ChallengeId = 99,
            Title = "Updated title"
        };

        // Act
        var result = await controller.Update(
            submittedChallenge, imageUpload: null);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        await repository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Update_WhenUserIsNotOwner_ReturnsForbidWithoutSavingAndLogsWarning()
    {
        // Arrange
        var repository = Substitute.For<IChallengeRepository>();
        var logger = Substitute.For<ILogger<ChallengeController>>();

        var storedChallenge = new Challenge
        {
            ChallengeId = 10,
            CreatedByUserId = 8,
            Title = "Original title",
            Description = "Original description"
        };

        repository.GetChallengeByIdAsync(10)
            .Returns(Task.FromResult<Challenge?>(storedChallenge));

        // User 7 is signed in; user 8 owns the challenge.
        var controller = CreateUpdateController(
            repository, logger, userId: 7);

        var submittedChallenge = new Challenge
        {
            ChallengeId = 10,
            CreatedByUserId = 7,
            Title = "Unauthorized change",
            Description = "Unauthorized description"
        };

        // Act
        var result = await controller.Update(
            submittedChallenge, imageUpload: null);

        // Assert
        Assert.IsType<ForbidResult>(result);

        Assert.Equal("Original title", storedChallenge.Title);
        Assert.Equal("Original description", storedChallenge.Description);
        Assert.Equal(8, storedChallenge.CreatedByUserId);

        await repository.DidNotReceive().SaveChangesAsync();

        var warning = Assert.Single(
            logger.ReceivedCalls(),
            call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                Equals(call.GetArguments()[0], LogLevel.Warning));

        Assert.Contains(
            "denied challenge management access",
            warning.GetArguments()[2]?.ToString() ?? "");
    }

    // Shared setup for these two tests.
    // Both error paths return before querying the image service.
    private static ChallengeController CreateUpdateController(
        IChallengeRepository repository,
        ILogger<ChallengeController> logger,
        int userId)
    {
        var controller = new ChallengeController(
            repository,
            Substitute.For<IAttemptRepository>(),
            Substitute.For<IImageService>(),
            Substitute.For<IAchievementService>(),
            logger);

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

        return controller;
    }

    [Fact]
    public async Task Create_WhenInputIsValid_SavesDraftAndRedirectsWithInformationLog()
    {
        // Arrange
        var repository = Substitute.For<IChallengeRepository>();
        var logger = Substitute.For<ILogger<ChallengeController>>();
        var achievementService = Substitute.For<IAchievementService>();

        var imageService = Substitute.For<IImageService>();
        imageService.IsAllowedImageAsync(string.Empty, 7)
            .Returns(Task.FromResult(true));

        Challenge? savedChallenge = null;

        // Simulate saving and assigning a database-generated ID.
        repository.CreateChallengeAsync(Arg.Any<Challenge>())
            .Returns(call =>
            {
                savedChallenge = call.Arg<Challenge>();
                savedChallenge.ChallengeId = 42;

                return Task.FromResult(true);
            });

        achievementService.CheckAchievementsAsync(7)
            .Returns(Task.CompletedTask);

        var controller = new ChallengeController(
            repository,
            Substitute.For<IAttemptRepository>(),
            imageService,
            achievementService,
            logger);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "7")
            },
            authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var submittedChallenge = new Challenge
        {
            Title = "A valid challenge",
            Description = "A valid challenge description.",
            TimeLimitMinutes = 10,
            ImageUrl = "",

            // Submitted values must not override server-controlled fields.
            CreatedByUserId = 99,
            IsPublished = true,
            IsCore = true,
            MaxPoints = 999
        };

        // Act: an empty image selection is allowed; no upload is supplied.
        var result = await controller.Create(
            submittedChallenge,
            imageUpload: null);

        // Assert: create exactly one challenge with the correct values.
        await repository.Received(1)
            .CreateChallengeAsync(Arg.Any<Challenge>());

        Assert.NotNull(savedChallenge);

        Assert.Equal("A valid challenge", savedChallenge.Title);
        Assert.Equal(
            "A valid challenge description.",
            savedChallenge.Description);

        Assert.Equal(10, savedChallenge.TimeLimitMinutes);
        Assert.Equal(7, savedChallenge.CreatedByUserId);
        Assert.False(savedChallenge.IsPublished);
        Assert.False(savedChallenge.IsCore);
        Assert.Equal(0, savedChallenge.MaxPoints);

        // Redirect to the newly created challenge's details.
        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(42, redirect.RouteValues!["id"]);

        await achievementService.Received(1)
            .CheckAchievementsAsync(7);

        // Successful creation must produce an information log.
        var informationLog = Assert.Single(
            logger.ReceivedCalls(),
            call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                Equals(call.GetArguments()[0], LogLevel.Information));

        Assert.Contains(
            "was created by user",
            informationLog.GetArguments()[2]?.ToString() ?? "");
    }
}