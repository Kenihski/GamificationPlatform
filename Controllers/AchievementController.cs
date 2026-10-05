using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    [Authorize]
    public class AchievementController : Controller
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<AchievementController> _logger;

        public AchievementController(
            IAchievementRepository achievementRepository,
            IUserRepository userRepository,
            IAchievementService achievementService,
            ILogger<AchievementController> logger)
        {
            _achievementRepository = achievementRepository;
            _userRepository = userRepository;
            _achievementService = achievementService;
            _logger = logger;
        }

        // Shows the achievement page for the logged-in user.
        // Admins are shown the achievement management page instead.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (user.IsAdmin)
            {
                return await AdminIndex();
            }

            return await UserIndex(userId, user.Username);
        }

        // Builds the personal achievement page for a normal user.
        private async Task<IActionResult> UserIndex(
            int userId,
            string username)
        {
            // Only published achievements are shown to users.
            // Achievements are grouped by type and requirement.
            var achievements =
                (await _achievementRepository.GetActiveAsync())
                .OrderBy(a => GetAchievementCategoryOrder(a))
                .ThenBy(a => a.RequirementValue)
                .ThenBy(a => a.AchievementId)
                .ToList();

            var userAchievements =
                (await _achievementRepository
                    .GetUserAchievementsAsync(userId))
                .Where(ua => ua.Achievement.IsActive)
                .ToList();

            // Goals do not contribute Achievement Points.
            int totalPoints =
                userAchievements
                    .Where(ua => !ua.Achievement.IsGoal)
                    .Sum(ua => ua.Achievement.Points);

            // Only regular achievements contribute to maximum AP.
            int maxPoints =
                achievements
                    .Where(a => !a.IsGoal)
                    .Sum(a => a.Points);

            var viewModel =
                new AchievementsViewModel
                {
                    Username = username,
                    TotalPoints = totalPoints,
                    MaxPoints = maxPoints,
                    UnlockedCount = userAchievements.Count,
                    TotalAchievements = achievements.Count
                };

            // Creates one row for every published achievement.
            foreach (var achievement in achievements)
            {
                var unlocked =
                    userAchievements.FirstOrDefault(
                        ua =>
                            ua.AchievementId ==
                            achievement.AchievementId);

                viewModel.Achievements.Add(
                    new AchievementItemViewModel
                    {
                        AchievementId =
                            achievement.AchievementId,
                        Name = achievement.Name,
                        Description =
                            achievement.Description,
                        Points = achievement.Points,
                        IsGoal = achievement.IsGoal,
                        IsUnlocked = unlocked != null,
                        UnlockedAt = unlocked?.UnlockedAt
                    });
            }

            SetMilestone(viewModel);

            _logger.LogInformation(
                "Achievements page viewed by user {UserId}.",
                userId);

            return View("Index", viewModel);
        }

        // Shows all achievements to admins.
        private async Task<IActionResult> AdminIndex()
        {
            // Achievements are grouped by type and requirement.
            var achievements =
                (await _achievementRepository.GetAllAsync())
                .OrderBy(a => GetAchievementCategoryOrder(a))
                .ThenBy(a => a.RequirementValue)
                .ThenBy(a => a.AchievementId)
                .ToList();

            var viewModel =
                achievements.Select(
                    achievement =>
                        new AchievementAdminViewModel
                        {
                            AchievementId =
                                achievement.AchievementId,
                            Name = achievement.Name,
                            Description =
                                achievement.Description,
                            Points = achievement.Points,
                            Type = achievement.Type,
                            RequirementValue =
                                achievement.RequirementValue,
                            IsActive =
                                achievement.IsActive,
                            IsGoal =
                                achievement.IsGoal
                        })
                    .ToList();

            _logger.LogInformation(
                "Achievement management page viewed by admin.");

            return View("AdminIndex", viewModel);
        }

        // Shows the form for creating a new achievement.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var viewModel =
                new AchievementAdminViewModel
                {
                    IsActive = true,
                    Points = 5
                };

            return View(viewModel);
        }

        // Creates a new regular achievement.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AchievementAdminViewModel viewModel)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            // Only regular achievement types can be created by admins.
            var validTypes = new[]
            {
                "ChallengesCompleted",
                "ScorePercentage",
                "SecondAttemptImprovement",
                "SameChallengeAttempts",
                "ChallengesCreated",
                "CreatorUniqueParticipants",
                "ChallengeLeaderboardPosition",
                "CoreLeaderboardPosition"
            };

            if (!validTypes.Contains(viewModel.Type))
            {
                ModelState.AddModelError(
                    nameof(viewModel.Type),
                    "Invalid achievement type.");
            }

            // Only predefined AP values are allowed.
            var validPoints = new[]
            {
                5,
                10,
                15,
                20
            };

            if (!validPoints.Contains(viewModel.Points))
            {
                ModelState.AddModelError(
                    nameof(viewModel.Points),
                    "AP must be 5, 10, 15 or 20.");
            }

            // Percentage requirements cannot be above 100.
            if (viewModel.Type == "ScorePercentage" &&
                viewModel.RequirementValue > 100)
            {
                ModelState.AddModelError(
                    nameof(viewModel.RequirementValue),
                    "Percentage requirements cannot be above 100.");
            }

            // Goals are system-defined and cannot be created manually.
            if (viewModel.IsGoal)
            {
                ModelState.AddModelError(
                    nameof(viewModel.IsGoal),
                    "System goals cannot be created manually.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var achievement =
                new Achievement
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    Points = viewModel.Points,
                    Type = viewModel.Type,
                    RequirementValue =
                        viewModel.RequirementValue,
                    IsActive = viewModel.IsActive,

                    // Admin-created achievements are always regular.
                    IsGoal = false
                };

            await _achievementRepository.AddAsync(achievement);
            await _achievementRepository.SaveChangesAsync();

            // Existing normal users are checked immediately
            // when a published achievement is created.
            if (achievement.IsActive)
            {
                await CheckAchievementsForAllUsersAsync();
            }

            _logger.LogInformation(
                "Achievement {AchievementName} created by admin {UserId}.",
                achievement.Name,
                userId);

            return RedirectToAction(nameof(Index));
        }

        // Shows the form for editing an existing achievement.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be edited by admins.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be edited.");
            }

            var viewModel =
                new AchievementAdminViewModel
                {
                    AchievementId =
                        achievement.AchievementId,
                    Name =
                        achievement.Name,
                    Description =
                        achievement.Description,
                    Points =
                        achievement.Points,
                    Type =
                        achievement.Type,
                    RequirementValue =
                        achievement.RequirementValue,
                    IsActive =
                        achievement.IsActive,
                    IsGoal =
                        achievement.IsGoal
                };

            return View(viewModel);
        }

        // Updates an existing achievement.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AchievementAdminViewModel viewModel)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(
                    viewModel.AchievementId);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be edited by admins.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be edited.");
            }

            var validTypes = new[]
            {
                "ChallengesCompleted",
                "ScorePercentage",
                "SecondAttemptImprovement",
                "SameChallengeAttempts",
                "ChallengesCreated",
                "CreatorUniqueParticipants",
                "ChallengeLeaderboardPosition",
                "CoreLeaderboardPosition"
            };

            if (!validTypes.Contains(viewModel.Type))
            {
                ModelState.AddModelError(
                    nameof(viewModel.Type),
                    "Invalid achievement type.");
            }

            var validPoints = new[]
            {
                5,
                10,
                15,
                20
            };

            if (!validPoints.Contains(viewModel.Points))
            {
                ModelState.AddModelError(
                    nameof(viewModel.Points),
                    "AP must be 5, 10, 15 or 20.");
            }

            if (viewModel.Type == "ScorePercentage" &&
                viewModel.RequirementValue > 100)
            {
                ModelState.AddModelError(
                    nameof(viewModel.RequirementValue),
                    "Percentage requirements cannot be above 100.");
            }

            if (!ModelState.IsValid)
            {
                // Keep database values for properties
                // that are not editable in the form.
                viewModel.IsActive =
                    achievement.IsActive;

                viewModel.IsGoal =
                    achievement.IsGoal;

                return View(viewModel);
            }

            achievement.Name =
                viewModel.Name;

            achievement.Description =
                viewModel.Description;

            achievement.Points =
                viewModel.Points;

            achievement.Type =
                viewModel.Type;

            achievement.RequirementValue =
                viewModel.RequirementValue;

            // Publishing is handled separately.
            // Edit does not change IsActive or IsGoal.

            _achievementRepository.Update(achievement);
            await _achievementRepository.SaveChangesAsync();

            // A changed requirement may now be satisfied
            // by existing users.
            if (achievement.IsActive)
            {
                await CheckAchievementsForAllUsersAsync();
            }

            _logger.LogInformation(
                "Achievement {AchievementId} edited by admin {UserId}.",
                achievement.AchievementId,
                userId);

            return RedirectToAction(nameof(Index));
        }

        // Shows the delete confirmation page.
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be deleted.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be deleted.");
            }

            var viewModel =
                new AchievementAdminViewModel
                {
                    AchievementId =
                        achievement.AchievementId,
                    Name =
                        achievement.Name,
                    Description =
                        achievement.Description,
                    Points =
                        achievement.Points,
                    Type =
                        achievement.Type,
                    RequirementValue =
                        achievement.RequirementValue,
                    IsActive =
                        achievement.IsActive,
                    IsGoal =
                        achievement.IsGoal
                };

            return View(viewModel);
        }

        // Deletes an achievement after confirmation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be deleted.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be deleted.");
            }

            _achievementRepository.Delete(achievement);
            await _achievementRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Achievement {AchievementId} deleted by admin {UserId}.",
                achievement.AchievementId,
                userId);

            return RedirectToAction(nameof(Index));
        }

        // Publishes a regular achievement so users can unlock it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be changed by admins.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be changed.");
            }

            achievement.IsActive = true;

            _achievementRepository.Update(achievement);
            await _achievementRepository.SaveChangesAsync();

            // Existing normal users are checked when
            // an achievement becomes available.
            await CheckAchievementsForAllUsersAsync();

            _logger.LogInformation(
                "Achievement {AchievementId} published by admin {UserId}.",
                id,
                userId);

            return RedirectToAction(nameof(Index));
        }

        // Unpublishes a regular achievement.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdString);

            var user =
                await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsAdmin)
            {
                return Forbid();
            }

            var achievement =
                await _achievementRepository.GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            // System goals cannot be changed by admins.
            if (achievement.IsGoal)
            {
                return BadRequest(
                    "System goals cannot be changed.");
            }

            achievement.IsActive = false;

            _achievementRepository.Update(achievement);
            await _achievementRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Achievement {AchievementId} unpublished by admin {UserId}.",
                id,
                userId);

            return RedirectToAction(nameof(Index));
        }

        // Checks achievement requirements for all normal users.
        private async Task CheckAchievementsForAllUsersAsync()
        {
            var users =
                await _userRepository.GetAllUsersAsync();

            foreach (var user in users)
            {
                // Admins do not participate in achievements.
                if (user.IsAdmin)
                {
                    continue;
                }

                await _achievementService.CheckAchievementsAsync(
                    user.UserId);
            }
        }

        // Defines the order in which achievement categories are displayed.
        private static int GetAchievementCategoryOrder(
            Achievement achievement)
        {
            // Milestone goals are always shown last.
            if (achievement.IsGoal)
            {
                return 7;
            }

            return achievement.Type switch
            {
                "ChallengesCompleted" => 1,

                "ScorePercentage" => 2,

                "SecondAttemptImprovement" => 3,
                "SameChallengeAttempts" => 3,

                "ChallengesCreated" => 4,
                "CreatorUniqueParticipants" => 4,

                "ChallengeLeaderboardPosition" => 5,

                "CoreLeaderboardPosition" => 6,

                _ => 99
            };
        }

        // Calculates the user's milestone and progress
        // from the currently available achievement points.
        private static void SetMilestone(
            AchievementsViewModel viewModel)
        {
            int maxPoints =
                viewModel.MaxPoints;

            int totalPoints =
                viewModel.TotalPoints;

            // Handles an empty achievement catalogue.
            if (maxPoints <= 0)
            {
                viewModel.MilestoneName =
                    "Getting Started";

                viewModel.MilestoneIcon =
                    "🌱";

                viewModel.MilestoneMessage =
                    "Every expert has to start somewhere.";

                viewModel.ProgressPercent = 0;
                viewModel.CurrentMilestonePoints = 0;
                viewModel.NextMilestonePoints = 0;

                return;
            }

            int bronze =
                (int)Math.Ceiling(maxPoints * 0.20);

            int silver =
                (int)Math.Ceiling(maxPoints * 0.40);

            int gold =
                (int)Math.Ceiling(maxPoints * 0.60);

            int diamond =
                (int)Math.Ceiling(maxPoints * 0.80);

            int legend =
                maxPoints;

            if (totalPoints >= legend)
            {
                viewModel.MilestoneName =
                    "Legend";

                viewModel.MilestoneIcon =
                    "👑";

                viewModel.MilestoneMessage =
                    "Congratulations! You have officially achieved maximum knowledge. There is clearly nothing left to learn.";

                viewModel.CurrentMilestonePoints =
                    legend;

                viewModel.NextMilestonePoints =
                    legend;

                viewModel.ProgressPercent = 100;

                return;
            }

            if (totalPoints >= diamond)
            {
                SetMilestoneValues(
                    viewModel,
                    "Diamond",
                    "💎",
                    "Maximum knowledge is dangerously close. Science is concerned.",
                    diamond,
                    legend);

                return;
            }

            if (totalPoints >= gold)
            {
                SetMilestoneValues(
                    viewModel,
                    "Gold",
                    "🥇",
                    "Impressive. People might start asking you for help now.",
                    gold,
                    diamond);

                return;
            }

            if (totalPoints >= silver)
            {
                SetMilestoneValues(
                    viewModel,
                    "Silver",
                    "🥈",
                    "Things are getting serious. You might actually know what you're doing.",
                    silver,
                    gold);

                return;
            }

            if (totalPoints >= bronze)
            {
                SetMilestoneValues(
                    viewModel,
                    "Bronze",
                    "🥉",
                    "Not bad. You definitely know more than when you started.",
                    bronze,
                    silver);

                return;
            }

            SetMilestoneValues(
                viewModel,
                "Getting Started",
                "🌱",
                "Every expert has to start somewhere.",
                0,
                bronze);
        }

        // Calculates progress inside the current milestone.
        private static void SetMilestoneValues(
            AchievementsViewModel viewModel,
            string name,
            string icon,
            string message,
            int currentMilestone,
            int nextMilestone)
        {
            viewModel.MilestoneName =
                name;

            viewModel.MilestoneIcon =
                icon;

            viewModel.MilestoneMessage =
                message;

            viewModel.CurrentMilestonePoints =
                currentMilestone;

            viewModel.NextMilestonePoints =
                nextMilestone;

            int milestoneSize =
                nextMilestone - currentMilestone;

            int pointsInMilestone =
                viewModel.TotalPoints -
                currentMilestone;

            if (milestoneSize <= 0)
            {
                viewModel.ProgressPercent = 100;
                return;
            }

            double progress =
                (double)pointsInMilestone /
                milestoneSize * 100;

            viewModel.ProgressPercent =
                Math.Clamp(
                    (int)Math.Round(progress),
                    0,
                    100);
        }
    }
}