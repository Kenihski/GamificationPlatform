
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;
using GamificationPlatform.Services;

namespace GamificationPlatform.DAL
{
    public static class DBInit
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var serviceScope =
                app.ApplicationServices.CreateScope();

            ChallengeDbContext context =
                serviceScope.ServiceProvider
                    .GetRequiredService<ChallengeDbContext>();

            var userManager = serviceScope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

            var roleManager = serviceScope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole<int>>>();

            var achievementService =
                serviceScope.ServiceProvider
                    .GetRequiredService<IAchievementService>();

            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GamificationPlatform.DAL.DBInit");

            logger.LogInformation(
                "Development database initialization started.");

            try
            {
                await SeedDatabaseAsync(context, userManager, roleManager);

                // Update achievements for the seeded users.
                var userIds =
                    await context.Users
                        .Select(u => u.Id)
                        .ToListAsync();

                foreach (var userId in userIds)
                {
                    await achievementService
                        .CheckAchievementsAsync(userId);
                }

                logger.LogInformation(
                    "Development database initialization completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Development database initialization failed.");

                throw;
            }
        }

        private static async Task SeedDatabaseAsync(
            ChallengeDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // Reset the development database.
            context.Database.EnsureDeleted();
            context.Database.Migrate();
            ClearUploadedImages();

            await UserSeeder.SeedAsync(context, userManager, roleManager);
            AchievementSeeder.Seed(context);
            ChallengeSeeder.Seed(context);
            QuestionSeeder.Seed(context);
            AttemptSeeder.Seed(context);
            await ActivitySeeder.SeedAsync(context, userManager);
        }

        private static void ClearUploadedImages()
        {
            string uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "images");

            if (!Directory.Exists(uploadsPath))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(uploadsPath))
            {
                File.Delete(filePath);
            }
        }
    }
}
