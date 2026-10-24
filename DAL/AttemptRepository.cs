using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to challenge attempts.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class AttemptRepository : IAttemptRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;
        private readonly ILogger<AttemptRepository> _logger;

        public AttemptRepository(
            ChallengeDbContext challengeDbContext,
            ILogger<AttemptRepository> logger)
        {
            _challengeDbContext = challengeDbContext;
            _logger = logger;
        }

        // Returns all completed attempts for a user
        // in a specific challenge.
        public async Task<List<ChallengeAttempt>> GetCompletedAttemptsAsync(
            int userId,
            int challengeId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.UserChallenge.ChallengeId == challengeId &&
                        a.Completed)
                    .OrderByDescending(a => a.CompletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}, ChallengeId {ChallengeId}).",
                    nameof(GetCompletedAttemptsAsync),
                    nameof(AttemptRepository),
                    userId,
                    challengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns the IDs of all challenges
        // completed by a specific user.
        public async Task<List<int>> GetCompletedChallengeIdsAsync(
            int userId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.Completed)
                    .Select(a =>
                        a.UserChallenge.ChallengeId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetCompletedChallengeIdsAsync),
                    nameof(AttemptRepository),
                    userId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns all completed attempts for a challenge
        // together with the user who made each attempt.
        public async Task<List<ChallengeAttempt>> GetCompletedAttemptsForChallengeAsync(
            int challengeId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.User)
                    .Where(a =>
                        a.UserChallenge.ChallengeId == challengeId &&
                        a.Completed)
                    .ToListAsync();
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (ChallengeId {ChallengeId}).",
                    nameof(GetCompletedAttemptsForChallengeAsync),
                    nameof(AttemptRepository),
                    challengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns all completed attempts
        // with the related user and challenge.
        public async Task<List<ChallengeAttempt>> GetAllCompletedAttemptsAsync()
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.User)
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.Challenge)
                            .ThenInclude(c => c.Questions)
                    .Where(a => a.Completed)
                    .ToListAsync();
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetAllCompletedAttemptsAsync),
                    nameof(AttemptRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a completed attempt with all related
        // data needed for the attempt details page.
        public async Task<ChallengeAttempt?> GetAttemptDetailsAsync(
            int attemptId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.User)
                    .Include(a => a.UserChallenge)
                        .ThenInclude(uc => uc.Challenge)
                            .ThenInclude(c => c.Questions)
                    .Include(a => a.Answers)
                        .ThenInclude(answer => answer.Question)
                            .ThenInclude(question => question.Options)
                    .Include(a => a.Answers)
                        .ThenInclude(answer => answer.Question)
                            .ThenInclude(question => question.AcceptedAnswers)
                    .Include(a => a.Answers)
                        .ThenInclude(answer => answer.SelectedOptions)
                            .ThenInclude(selected =>
                                selected.QuestionOption)
                    .FirstOrDefaultAsync(a =>
                        a.ChallengeAttemptId == attemptId &&
                        a.Completed);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (AttemptId {AttemptId}).",
                    nameof(GetAttemptDetailsAsync),
                    nameof(AttemptRepository),
                    attemptId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns the relationship between
        // a user and a challenge.
        public async Task<UserChallenge?> GetUserChallengeAsync(
            int userId,
            int challengeId)
        {
            try
            {
                return await _challengeDbContext
                    .UserChallenges
                    .FirstOrDefaultAsync(uc =>
                        uc.UserId == userId &&
                        uc.ChallengeId == challengeId);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}, ChallengeId {ChallengeId}).",
                    nameof(GetUserChallengeAsync),
                    nameof(AttemptRepository),
                    userId,
                    challengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Creates a relationship between
        // a user and a challenge.
        public async Task<bool> CreateUserChallengeAsync(
            UserChallenge userChallenge)
        {
            _challengeDbContext
                .UserChallenges
                .Add(userChallenge);

            return await SaveChangesAsync();
        }

        // Returns the newest unfinished attempt
        // for a user challenge.
        public async Task<ChallengeAttempt?> GetUnfinishedAttemptAsync(
            int userChallengeId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Where(a =>
                        a.UserChallengeId == userChallengeId &&
                        !a.Completed)
                    .OrderByDescending(a =>
                        a.StartedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserChallengeId {UserChallengeId}).",
                    nameof(GetUnfinishedAttemptAsync),
                    nameof(AttemptRepository),
                    userChallengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Creates a new challenge attempt.
        public async Task<bool> CreateAttemptAsync(
            ChallengeAttempt attempt)
        {
            _challengeDbContext
                .ChallengeAttempts
                .Add(attempt);

            return await SaveChangesAsync();
        }

        // Returns an unfinished attempt only when
        // it belongs to the correct user and challenge.
        public async Task<ChallengeAttempt?> GetValidUnfinishedAttemptAsync(
            int attemptId,
            int userId,
            int challengeId)
        {
            try
            {
                return await _challengeDbContext
                    .ChallengeAttempts
                    .Include(a => a.UserChallenge)
                    .FirstOrDefaultAsync(a =>
                        a.ChallengeAttemptId == attemptId &&
                        a.UserChallenge.UserId == userId &&
                        a.UserChallenge.ChallengeId == challengeId &&
                        !a.Completed);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (AttemptId {AttemptId}, UserId {UserId}, ChallengeId {ChallengeId}).",
                    nameof(GetValidUnfinishedAttemptAsync),
                    nameof(AttemptRepository),
                    attemptId,
                    userId,
                    challengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Adds an answer to the current attempt.
        // Changes are saved together after all answers
        // and the completed attempt have been updated.
        public void AddAttemptAnswer(
            AttemptAnswer attemptAnswer)
        {
            _challengeDbContext
                .AttemptAnswers
                .Add(attemptAnswer);
        }

        // Removes previous attempts and answers when
        // the content of a challenge changes.
        public async Task DeleteChallengeHistoryAsync(
            int challengeId)
        {
            try
            {
                var attempts =
                    await _challengeDbContext
                        .ChallengeAttempts
                        .Include(a => a.Answers)
                        .Where(a =>
                            a.UserChallenge.ChallengeId ==
                                challengeId)
                        .ToListAsync();

                // Delete answers first because they
                // reference attempts and answer options.
                foreach (var attempt in attempts)
                {
                    _challengeDbContext
                        .AttemptAnswers
                        .RemoveRange(attempt.Answers);
                }

                // Delete the attempts after their answers.
                _challengeDbContext
                    .ChallengeAttempts
                    .RemoveRange(attempts);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (ChallengeId {ChallengeId}).",
                    nameof(DeleteChallengeHistoryAsync),
                    nameof(AttemptRepository),
                    challengeId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Saves pending database changes.
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _challengeDbContext
                    .SaveChangesAsync() > 0;
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(SaveChangesAsync),
                    nameof(AttemptRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }
    }
}