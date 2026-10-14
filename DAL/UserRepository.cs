
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to users.
    public class UserRepository : IUserRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ChallengeDbContext challengeDbContext,
            UserManager<User> userManager,
            ILogger<UserRepository> logger)
        {
            _challengeDbContext = challengeDbContext;
            _userManager = userManager;
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

                throw;
            }
        }

        // Returns a user by id.
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _userManager.FindByIdAsync(
                    id.ToString());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetUserByIdAsync),
                    nameof(UserRepository),
                    id);

                throw;
            }
        }

        // Returns a user with the related data
        // needed for the admin details page.
        public async Task<User?> GetUserWithDetailsAsync(int id)
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
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (UserId {UserId}).",
                    nameof(GetUserWithDetailsAsync),
                    nameof(UserRepository),
                    id);

                throw;
            }
        }

        // Returns a user by username.
        public async Task<User?> GetUserByUsernameAsync(
            string username)
        {
            try
            {
                return await _userManager.FindByNameAsync(
                    username.Trim());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetUserByUsernameAsync),
                    nameof(UserRepository));

                throw;
            }
        }

        // Returns a user by email.
        public async Task<User?> GetUserByEmailAsync(
            string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(
                    email.Trim());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetUserByEmailAsync),
                    nameof(UserRepository));

                throw;
            }
        }

        // Creates a new user.
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning(
                        "Failed to create user: {Errors}",
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(CreateUserAsync),
                    nameof(UserRepository));

                throw;
            }
        }

        // Updates an existing user.
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning(
                        "Failed to update user: {Errors}",
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(UpdateUserAsync),
                    nameof(UserRepository));

                throw;
            }
        }

        // Deletes an existing user.
        public async Task<bool> DeleteUserAsync(User user)
        {
            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning(
                        "Failed to delete user: {Errors}",
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(DeleteUserAsync),
                    nameof(UserRepository));

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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(SaveChangesAsync),
                    nameof(UserRepository));

                throw;
            }
        }
    }
}
