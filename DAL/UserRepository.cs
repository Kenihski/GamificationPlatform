using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to users.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class UserRepository : IUserRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ChallengeDbContext challengeDbContext,
            ILogger<UserRepository> logger)
        {
            _challengeDbContext = challengeDbContext;
            _logger = logger;
        }

        // Returns all users together with
        // the challenges they have created.
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _challengeDbContext.Users
                    .Include(u => u.CreatedChallenges)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetAllUsersAsync),
                    nameof(UserRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a user by id.
        public async Task<User?> GetUserByIdAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Users
                    .FirstOrDefaultAsync(u =>
                        u.UserId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetUserByIdAsync),
                    nameof(UserRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a user with the related data
        // needed for the admin details page.
        public async Task<User?> GetUserWithDetailsAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Users
                    .Include(u => u.CreatedChallenges)
                        .ThenInclude(c => c.Questions)
                    .Include(u => u.UserChallenges)
                        .ThenInclude(uc => uc.Challenge)
                    .Include(u => u.UserChallenges)
                        .ThenInclude(uc => uc.Attempts)
                    .FirstOrDefaultAsync(u =>
                        u.UserId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetUserWithDetailsAsync),
                    nameof(UserRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a user by username.
        public async Task<User?> GetUserByUsernameAsync(
            string username)
        {
            try
            {
                return await _challengeDbContext.Users
                    .FirstOrDefaultAsync(u =>
                        u.Username == username);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetUserByUsernameAsync),
                    nameof(UserRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a user by email.
        public async Task<User?> GetUserByEmailAsync(
            string email)
        {
            try
            {
                return await _challengeDbContext.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email == email);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetUserByEmailAsync),
                    nameof(UserRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Creates a new user.
        public async Task<bool> CreateUserAsync(
            User user)
        {
            _challengeDbContext.Users.Add(user);

            return await SaveChangesAsync();
        }

        // Updates an existing user.
        public async Task<bool> UpdateUserAsync(
            User user)
        {
            _challengeDbContext.Users.Update(user);

            return await SaveChangesAsync();
        }

        // Deletes an existing user.
        public async Task<bool> DeleteUserAsync(
            User user)
        {
            _challengeDbContext.Users.Remove(user);

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
                    nameof(UserRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }
    }
}
