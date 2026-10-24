using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.DAL;

public class AchievementLoggingTests
{
    [Fact]
    public async Task AddAsync_WhenOnlyTracked_DoesNotLogAchievementAsCreated()
    {
        // Stage an entity without persisting it to the database.
        var options = new DbContextOptionsBuilder<ChallengeDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new ChallengeDbContext(options);
        var logger = Substitute.For<ILogger<AchievementRepository>>();
        var repository = new AchievementRepository(context, logger);

        await repository.AddAsync(new Achievement
        {
            Name = "Example achievement",
            Description = "An example for the logging test."
        });

        // A successful database save has not happened.
        Assert.DoesNotContain(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Information));
    }
}
