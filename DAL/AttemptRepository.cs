using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to challenge attempts.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class AttemptRepository : IAttemptRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public AttemptRepository(
            ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }


        // Returns all completed attempts for a user
        // in a specific challenge.
        public async Task<List<ChallengeAttempt>> GetCompletedAttemptsAsync(
            int userId,
            int challengeId)
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


        // Returns all completed attempts for a challenge
        // together with the user who made each attempt.
        public async Task<List<ChallengeAttempt>> GetCompletedAttemptsForChallengeAsync(
            int challengeId)
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


        // Returns a completed attempt with all related
        // data needed for the attempt details page.
        public async Task<ChallengeAttempt?> GetAttemptDetailsAsync(
            int attemptId)
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
                .FirstOrDefaultAsync(a =>
                    a.ChallengeAttemptId == attemptId &&
                    a.Completed);
        }


        // Returns the relationship between
        // a user and a challenge.
        public async Task<UserChallenge?> GetUserChallengeAsync(
            int userId,
            int challengeId)
        {
            return await _challengeDbContext
                .UserChallenges
                .FirstOrDefaultAsync(uc =>
                    uc.UserId == userId &&
                    uc.ChallengeId == challengeId);
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
            return await _challengeDbContext
                .ChallengeAttempts
                .Where(a =>
                    a.UserChallengeId == userChallengeId &&
                    !a.Completed)
                .OrderByDescending(a =>
                    a.StartedAt)
                .FirstOrDefaultAsync();
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
            return await _challengeDbContext
                .ChallengeAttempts
                .Include(a => a.UserChallenge)
                .FirstOrDefaultAsync(a =>
                    a.ChallengeAttemptId == attemptId &&
                    a.UserChallenge.UserId == userId &&
                    a.UserChallenge.ChallengeId == challengeId &&
                    !a.Completed);
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


        // Saves pending database changes.
        public async Task<bool> SaveChangesAsync()
        {
            return await _challengeDbContext
                .SaveChangesAsync() > 0;
        }
    }
}