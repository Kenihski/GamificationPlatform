using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class HomeErrorControllerTests
{
    [Fact]
    public void Error_WhenUnhandledExceptionOccurs_ReturnsGeneric500AndRequestReference()
    {
        // Arrange: the error action does not need database access.
        var controller = CreateController(out _);
        controller.ControllerContext.HttpContext.TraceIdentifier = "request-500";

        // Act
        var result = controller.Error();

        // Assert: no technical exception details are shown.
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal(500, controller.Response.StatusCode);
        Assert.Equal(500, model.StatusCode);
        Assert.Equal("request-500", model.RequestId);
        Assert.Equal("Something went wrong", model.Title);
        Assert.DoesNotContain("stack", model.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(400, "Invalid request")]
    [InlineData(401, "Login required")]
    [InlineData(403, "Access denied")]
    [InlineData(404, "Page not found")]
    [InlineData(500, "Request failed")]
    [InlineData(200, "Page not found")]
    public void ErrorStatus_WhenStatusProvided_PreservesStatusAndLogsWarning(
        int requestedStatus, string expectedTitle)
    {
        var controller = CreateController(out var logger);
        controller.ControllerContext.HttpContext.TraceIdentifier = "request-status";
        var expectedStatus = requestedStatus < 400 ? 404 : requestedStatus;

        var result = controller.ErrorStatus(requestedStatus);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", view.ViewName);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal(expectedStatus, model.StatusCode);
        Assert.Equal(expectedStatus, controller.Response.StatusCode);
        Assert.Equal(expectedTitle, model.Title);
        Assert.Equal("request-status", model.RequestId);

        var warning = Assert.Single(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Warning));
        Assert.Contains("request-status",
            warning.GetArguments()[2]?.ToString() ?? "");
    }

    private static HomeController CreateController(
        out ILogger<HomeController> logger)
    {
        logger = Substitute.For<ILogger<HomeController>>();
        var controller = new HomeController(
            Substitute.For<IChallengeRepository>(),
            Substitute.For<IAttemptRepository>(),
            Substitute.For<IUserRepository>(),
            null!, // UserManager is unused by the error actions.
            Substitute.For<IAchievementRepository>(),
            logger);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }
}
