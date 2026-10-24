using System.Security.Claims;
using GamificationPlatform.DAL;
using GamificationPlatform.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GamificationPlatform.ViewComponents
{
    public class AchievementNotificationsViewComponent : ViewComponent
    {
        private readonly IAchievementRepository _achievementRepository;

        public AchievementNotificationsViewComponent(
            IAchievementRepository achievementRepository)
        {
            _achievementRepository = achievementRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Do not show achievement notifications to logged-out users.
            if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
            {
                return Content(string.Empty);
            }

            // Admins do not earn achievements.
            if (UserClaimsPrincipal.IsInRole("Admin"))
            {
                return Content(string.Empty);
            }

            var userIdClaim = UserClaimsPrincipal.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Content(string.Empty);
            }

            var unseenAchievements =
                await _achievementRepository
                    .GetUnseenUserAchievementsAsync(userId);

            var viewModel = unseenAchievements
                .Select(ua => new AchievementNotificationViewModel
                {
                    UserAchievementId = ua.UserAchievementId,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    Points = ua.Achievement.Points,
                    IsGoal = ua.Achievement.IsGoal
                })
                .ToList();

            return View(
            "~/Views/Shared/_AchievementNotification.cshtml",
            viewModel);
        }
    }
}