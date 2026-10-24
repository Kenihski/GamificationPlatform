using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public interface IAchievementRepository
    {
        Task<IEnumerable<Achievement>> GetAllAsync();

        Task<IEnumerable<Achievement>> GetActiveAsync();

        Task<Achievement?> GetByIdAsync(int id);

        Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(
            int userId);

        Task<IEnumerable<UserAchievement>> GetUnseenUserAchievementsAsync(
            int userId);

        Task<bool> HasAchievementAsync(
            int userId,
            int achievementId);

        Task AddAsync(Achievement achievement);

        void Update(Achievement achievement);

        void Delete(Achievement achievement);

        Task AddUserAchievementAsync(
            UserAchievement userAchievement);

        Task<bool> MarkNotificationSeenAsync(
            int userAchievementId,
            int userId);

        Task SaveChangesAsync();
    }
}