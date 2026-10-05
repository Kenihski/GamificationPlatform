namespace GamificationPlatform.Services
{
    public interface IAchievementService
    {
        Task CheckAchievementsAsync(int userId);
    }
}