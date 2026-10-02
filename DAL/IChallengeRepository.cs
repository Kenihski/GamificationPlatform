using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Defines the database operations available for challenges.
    // This interface separates the controllers from the database implementation.
    public interface IChallengeRepository
    {
        // Returns all published challenges.
        Task<List<Challenge>> GetPublishedChallengesAsync();

        // Returns all challenges created by a specific user.
        Task<List<Challenge>> GetChallengesByUserIdAsync(
            int userId);

        // Returns a challenge by its id.
        Task<Challenge?> GetChallengeByIdAsync(
            int id);

        // Returns all challenges.
        Task<List<Challenge>> GetAllChallengesAsync();

        // Returns a challenge together with its questions.
        Task<Challenge?> GetChallengeWithQuestionsAsync(
            int id);

        // Returns a challenge with questions and answer options.
        Task<Challenge?> GetChallengeWithQuestionsAndOptionsAsync(
            int id);

        // Creates a new challenge.
        Task<bool> CreateChallengeAsync(
            Challenge challenge);

        // Updates an existing challenge.
        Task<bool> UpdateChallengeAsync(
            Challenge challenge);

        // Deletes an existing challenge.
        Task<bool> DeleteChallengeAsync(
            Challenge challenge);

        // Saves pending changes to the database.
        Task<bool> SaveChangesAsync();
    }
}