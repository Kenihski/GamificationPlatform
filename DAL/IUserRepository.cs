using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Defines the database operations available for users.
    // This interface separates the controllers from the database implementation.
    public interface IUserRepository
    {
        // Returns all users together with their created challenges.
        Task<List<User>> GetAllUsersAsync();

        // Returns a user by id.
        Task<User?> GetUserByIdAsync(
            int id);

        // Returns a user with the data needed
        // for the admin details page.
        Task<User?> GetUserWithDetailsAsync(
            int id);

        // Returns a user by username.
        Task<User?> GetUserByUsernameAsync(
            string username);

        // Returns a user by email.
        Task<User?> GetUserByEmailAsync(
            string email);

        // Creates a new user.
        Task<bool> CreateUserAsync(
            User user);

        // Updates an existing user.
        Task<bool> UpdateUserAsync(
            User user);

        // Deletes an existing user.
        Task<bool> DeleteUserAsync(
            User user);

        // Saves pending changes to the database.
        Task<bool> SaveChangesAsync();
    }
}