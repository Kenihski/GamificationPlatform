using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to challenges.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;
        private readonly ILogger<ChallengeRepository> _logger;

        public ChallengeRepository(
            ChallengeDbContext challengeDbContext,
            ILogger<ChallengeRepository> logger)
        {
            _challengeDbContext = challengeDbContext;
            _logger = logger;
        }

        // Returns all published challenges.
        public async Task<List<Challenge>> GetPublishedChallengesAsync()
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .Where(c => c.IsPublished)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetPublishedChallengesAsync),
                    nameof(ChallengeRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns all challenges created by a specific user.
        public async Task<List<Challenge>> GetChallengesByUserIdAsync(
            int userId)
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .Where(c => c.CreatedByUserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetChallengesByUserIdAsync),
                    nameof(ChallengeRepository),
                    userId);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a challenge without loading related data.
        public async Task<Challenge?> GetChallengeByIdAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (ChallengeId {ChallengeId}).",
                    nameof(GetChallengeByIdAsync),
                    nameof(ChallengeRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a challenge together with its questions.
        public async Task<Challenge?> GetChallengeWithQuestionsAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (ChallengeId {ChallengeId}).",
                    nameof(GetChallengeWithQuestionsAsync),
                    nameof(ChallengeRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a challenge with questions and answer options.
        public async Task<Challenge?> GetChallengeWithQuestionsAndOptionsAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(c =>
                        c.ChallengeId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (ChallengeId {ChallengeId}).",
                    nameof(GetChallengeWithQuestionsAndOptionsAsync),
                    nameof(ChallengeRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns all challenges.
        public async Task<List<Challenge>> GetAllChallengesAsync()
        {
            try
            {
                return await _challengeDbContext.Challenges
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetAllChallengesAsync),
                    nameof(ChallengeRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Adds a new challenge to the database.
        public async Task<bool> CreateChallengeAsync(
            Challenge challenge)
        {
            _challengeDbContext.Challenges.Add(challenge);

            return await SaveChangesAsync();
        }

        // Updates an existing challenge.
        public async Task<bool> UpdateChallengeAsync(
            Challenge challenge)
        {
            _challengeDbContext.Challenges.Update(challenge);

            return await SaveChangesAsync();
        }

        // Removes a challenge from the database.
        public async Task<bool> DeleteChallengeAsync(
            Challenge challenge)
        {
            _challengeDbContext.Challenges.Remove(challenge);

            return await SaveChangesAsync();
        }

        // Saves pending database changes.
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _challengeDbContext
                    .SaveChangesAsync() > 0;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(SaveChangesAsync),
                    nameof(ChallengeRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }
    }
}
