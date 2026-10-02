using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Defines the database operations related to challenge attempts.
    // This keeps attempt database access out of the controllers.
    public interface IAttemptRepository
    {
        // Returns all completed attempts for a user
        // in a specific challenge.
        Task<List<ChallengeAttempt>> GetCompletedAttemptsAsync(
            int userId,
            int challengeId);

        // Returns all completed attempts for a challenge
        // together with the user who made each attempt.
        Task<List<ChallengeAttempt>> GetCompletedAttemptsForChallengeAsync(
            int challengeId);

        // Returns a completed attempt with all data
        // needed to display the attempt details.
        Task<ChallengeAttempt?> GetAttemptDetailsAsync(
            int attemptId);

        // Returns the relationship between
        // a user and a challenge.
        Task<UserChallenge?> GetUserChallengeAsync(
            int userId,
            int challengeId);

        // Creates a relationship between
        // a user and a challenge.
        Task<bool> CreateUserChallengeAsync(
            UserChallenge userChallenge);

        // Returns the newest unfinished attempt
        // for a user challenge.
        Task<ChallengeAttempt?> GetUnfinishedAttemptAsync(
            int userChallengeId);

        // Creates a new challenge attempt.
        Task<bool> CreateAttemptAsync(
            ChallengeAttempt attempt);

        // Returns an unfinished attempt belonging
        // to a specific user and challenge.
        Task<ChallengeAttempt?> GetValidUnfinishedAttemptAsync(
            int attemptId,
            int userId,
            int challengeId);

        // Adds an answer to an attempt.
        void AddAttemptAnswer(
            AttemptAnswer attemptAnswer);

        // Removes previous attempts and answers when
        // the content of a challenge changes.
        Task DeleteChallengeHistoryAsync(
            int challengeId);

        // Saves pending changes to the database.
        Task<bool> SaveChangesAsync();
    }
}