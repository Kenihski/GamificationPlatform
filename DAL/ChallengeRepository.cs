using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to challenges.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public ChallengeRepository(
            ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        // Returns all published challenges.
        public async Task<List<Challenge>> GetPublishedChallengesAsync()
        {
            return await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .Where(c => c.IsPublished)
                .ToListAsync();
        }

        // Returns all challenges created by a specific user.
        public async Task<List<Challenge>> GetChallengesByUserIdAsync(
            int userId)
        {
            return await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .Where(c => c.CreatedByUserId == userId)
                .ToListAsync();
        }

        // Returns a challenge without loading related data.
        public async Task<Challenge?> GetChallengeByIdAsync(
            int id)
        {
            return await _challengeDbContext.Challenges
                .FirstOrDefaultAsync(c =>
                    c.ChallengeId == id);
        }

        // Returns a challenge together with its questions.
        public async Task<Challenge?> GetChallengeWithQuestionsAsync(
            int id)
        {
            return await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c =>
                    c.ChallengeId == id);
        }

        // Returns a challenge with questions and answer options.
        public async Task<Challenge?> GetChallengeWithQuestionsAndOptionsAsync(
            int id)
        {
            return await _challengeDbContext.Challenges
                .Include(c => c.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(c =>
                    c.ChallengeId == id);
        }

        // Returns all challenges.
        public async Task<List<Challenge>> GetAllChallengesAsync()
        {
            return await _challengeDbContext.Challenges
                .ToListAsync();
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
            return await _challengeDbContext
                .SaveChangesAsync() > 0;
        }
    }
}