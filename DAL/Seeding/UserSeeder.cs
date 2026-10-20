
using Microsoft.AspNetCore.Identity;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(
            ChallengeDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Users
            // --------------------------------
            var users = new List<User>
            {
                new User { Id = 1, UserName = "admin", Email = "admin@example.com" },
                new User { Id = 2, UserName = "Alice", Email = "alice@example.com" },
                new User { Id = 3, UserName = "Bob", Email = "bob@example.com" },
                new User { Id = 4, UserName = "Charlie", Email = "charlie@example.com" },
                new User { Id = 5, UserName = "Emma", Email = "emma@example.com" },
                new User { Id = 6, UserName = "David", Email = "david@example.com" }
            };

            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<int>("Admin"));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Password123!");

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create {user.UserName}: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            var adminRoleResult = await userManager.AddToRoleAsync(
                users[0], "Admin");

            if (!adminRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", adminRoleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
