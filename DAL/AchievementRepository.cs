using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.DAL
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly ChallengeDbContext _context;
        private readonly ILogger<AchievementRepository> _logger;

        public AchievementRepository(
            ChallengeDbContext context,
            ILogger<AchievementRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Achievement>> GetAllAsync()
        {
            return await _context.Achievements
                .OrderBy(a => a.AchievementId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Achievement>> GetActiveAsync()
        {
            return await _context.Achievements
                .Where(a => a.IsActive)
                .OrderBy(a => a.AchievementId)
                .ToListAsync();
        }

        public async Task<Achievement?> GetByIdAsync(int id)
        {
            return await _context.Achievements
                .FirstOrDefaultAsync(
                    a => a.AchievementId == id);
        }

        public async Task<IEnumerable<UserAchievement>>
            GetUserAchievementsAsync(int userId)
        {
            return await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .OrderBy(ua => ua.AchievementId)
                .ToListAsync();
        }

        public async Task<bool> HasAchievementAsync(
            int userId,
            int achievementId)
        {
            return await _context.UserAchievements
                .AnyAsync(ua =>
                    ua.UserId == userId &&
                    ua.AchievementId == achievementId);
        }

        public async Task AddAsync(Achievement achievement)
        {
            await _context.Achievements.AddAsync(achievement);

            _logger.LogInformation(
                "Achievement {AchievementName} created.",
                achievement.Name);
        }

        public void Update(Achievement achievement)
        {
            _context.Achievements.Update(achievement);

            _logger.LogInformation(
                "Achievement {AchievementId} updated.",
                achievement.AchievementId);
        }

        public void Delete(Achievement achievement)
        {
            _context.Achievements.Remove(achievement);

            _logger.LogInformation(
                "Achievement {AchievementId} deleted.",
                achievement.AchievementId);
        }

        public async Task AddUserAchievementAsync(
            UserAchievement userAchievement)
        {
            await _context.UserAchievements
                .AddAsync(userAchievement);

            _logger.LogInformation(
                "Achievement {AchievementId} unlocked by user {UserId}.",
                userAchievement.AchievementId,
                userAchievement.UserId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}