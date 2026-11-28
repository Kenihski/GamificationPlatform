using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class LeaderboardControllerTests
{
    // Published leaderboards are public; draft leaderboards require ownership or Admin.
    [Theory]
    [InlineData(false, null, false, false)]
    [InlineData(false, 8, false, false)]
    [InlineData(false, 7, false, true)]
    [InlineData(false, 8, true, true)]
    [InlineData(true, null, false, true)]
    [InlineData(true, 8, false, true)]
    public async Task Leaderboard_EnforcesDraftVisibility(
        bool isPublished, int? userId, bool isAdmin, bool expectedAccess)
    {
        const int challengeId = 12;
        var challenges = Substitute.For<IChallengeRepository>();
        var attempts = Substitute.For<IAttemptRepository>();
        var logger = Substitute.For<ILogger<LeaderboardController>>();
        var userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(), null, null, null, null,
            null, null, null, null);

        var challenge = new Challenge
        {
            ChallengeId = challengeId,
            CreatedByUserId = 7,
            IsPublished = isPublished,
            Title = "Leaderboard visibility test"
        };

        challenges.GetChallengeWithQuestionsAsync(challengeId)
            .Returns(Task.FromResult<Challenge?>(challenge));
        attempts.GetCompletedAttemptsForChallengeAsync(challengeId)
            .Returns(Task.FromResult(new List<ChallengeAttempt>()));
        userManager.GetUsersInRoleAsync("Admin")
            .Returns(Task.FromResult<IList<User>>(new List<User>()));

        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = userId.HasValue
            ? new ClaimsIdentity(claims, "Test")
            : new ClaimsIdentity();

        var controller = new LeaderboardController(
            challenges, attempts, userManager, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };

        var result = await controller.Leaderboard(challengeId);

        if (expectedAccess)
        {
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LeaderboardViewModel>(view.Model);
            Assert.Same(challenge, model.Challenge);
            await attempts.Received(1)
                .GetCompletedAttemptsForChallengeAsync(challengeId);
        }
        else
        {
            Assert.IsType<NotFoundResult>(result);
            await attempts.DidNotReceive()
                .GetCompletedAttemptsForChallengeAsync(Arg.Any<int>());

            Assert.Contains(logger.ReceivedCalls(), call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                Equals(call.GetArguments()[0], LogLevel.Warning) &&
                (call.GetArguments()[2]?.ToString() ?? "")
                    .Contains("Unauthorised access to unpublished challenge leaderboard"));
        }
    }
}
