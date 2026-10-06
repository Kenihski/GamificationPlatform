using System.Security.Claims;
using GamificationPlatform.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamificationPlatform.Controllers
{
    [Authorize]
    public class AchievementNotificationController : Controller
    {
        private readonly IAchievementRepository _achievementRepository;

        public AchievementNotificationController(
            IAchievementRepository achievementRepository)
        {
            _achievementRepository = achievementRepository;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSeen(
            int userAchievementId)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var markedAsSeen =
                await _achievementRepository
                    .MarkNotificationSeenAsync(
                        userAchievementId,
                        userId);

            if (!markedAsSeen)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}