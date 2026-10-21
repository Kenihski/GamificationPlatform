using GamificationPlatform.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.DAL;

public class AdditionalRepositoryTests
{
    [Fact]
    public async Task QuestionRepository_WhenSaveFails_LogsErrorAndRethrows()
    {
        var (context, exception) = FailingContext();
        using (context)
        {
            var logger = Substitute.For<ILogger<QuestionRepository>>();
            var repository = new QuestionRepository(context, logger);

            var actual = await Assert.ThrowsAsync<DbUpdateException>(
                () => repository.SaveChangesAsync());

            Assert.Same(exception, actual);
            AssertError(logger, exception, nameof(QuestionRepository),
                nameof(QuestionRepository.SaveChangesAsync));
        }
    }

    [Fact]
    public async Task UserRepository_WhenSaveFails_LogsErrorAndRethrows()
    {
        var (context, exception) = FailingContext();
        using (context)
        {
            var logger = Substitute.For<ILogger<UserRepository>>();
            // SaveChangesAsync does not access the Identity user manager.
            var repository = new UserRepository(context, null!, logger);

            var actual = await Assert.ThrowsAsync<DbUpdateException>(
                () => repository.SaveChangesAsync());

            Assert.Same(exception, actual);
            AssertError(logger, exception, nameof(UserRepository),
                nameof(UserRepository.SaveChangesAsync));
        }
    }

    [Fact]
    public async Task AttemptRepository_WhenSaveFails_LogsErrorAndRethrows()
    {
        var (context, exception) = FailingContext();
        using (context)
        {
            var logger = Substitute.For<ILogger<AttemptRepository>>();
            var repository = new AttemptRepository(context, logger);

            var actual = await Assert.ThrowsAsync<DbUpdateException>(
                () => repository.SaveChangesAsync());

            Assert.Same(exception, actual);
            AssertError(logger, exception, nameof(AttemptRepository),
                nameof(AttemptRepository.SaveChangesAsync));
        }
    }

    private static (ChallengeDbContext, DbUpdateException) FailingContext()
    {
        var options = new DbContextOptionsBuilder<ChallengeDbContext>().Options;
        var context = Substitute.For<ChallengeDbContext>(options);
        var exception = new DbUpdateException("Simulated failure");
        context.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(exception));
        return (context, exception);
    }

    private static void AssertError<TRepository>(
        ILogger<TRepository> logger,
        DbUpdateException expected,
        string repositoryName,
        string operationName)
    {
        var log = Assert.Single(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Error));

        Assert.Same(expected, log.GetArguments()[3]);
        var message = log.GetArguments()[2]?.ToString() ?? "";
        Assert.Contains(repositoryName, message);
        Assert.Contains(operationName, message);
    }
}
