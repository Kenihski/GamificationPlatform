using GamificationPlatform.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.DAL;

public class ChallengeRepositoryTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenDatabaseFails_LogsErrorAndRethrowsSameException()
    {
        // Arrange: substitute the context and logger.
        var options =
            new DbContextOptionsBuilder<ChallengeDbContext>().Options;

        using var context =
            Substitute.For<ChallengeDbContext>(options);

        var logger =
            Substitute.For<ILogger<ChallengeRepository>>();

        var expectedException =
            new DbUpdateException("Simulated database save failure.");

        context.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(expectedException));

        // Test the actual repository, not a substitute repository.
        var repository = new ChallengeRepository(context, logger);

        // Act: capture the exception propagated by the repository.
        var actualException = await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.SaveChangesAsync());

        // Assert: the failure must not be swallowed or replaced.
        Assert.Same(expectedException, actualException);

        // Exactly one Error-level event must be recorded.
        var errorLog = Assert.Single(
            logger.ReceivedCalls(),
            call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                Equals(call.GetArguments()[0], LogLevel.Error));

        // The log must include the original exception.
        Assert.Same(
            expectedException,
            errorLog.GetArguments()[3]);

        // Identify the failed operation and repository.
        string message =
            errorLog.GetArguments()[2]?.ToString() ?? "";

        Assert.Contains(
            nameof(ChallengeRepository.SaveChangesAsync),
            message);

        Assert.Contains(
            nameof(ChallengeRepository),
            message);
    }
}