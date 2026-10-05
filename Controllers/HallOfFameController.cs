using Microsoft.AspNetCore.Mvc;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class HallOfFameController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IAchievementRepository _achievementRepository;
        private readonly ILogger<HallOfFameController> _logger;

        public HallOfFameController(
            IUserRepository userRepository,
            IAchievementRepository achievementRepository,
            ILogger<HallOfFameController> logger)
        {
            _userRepository = userRepository;
            _achievementRepository = achievementRepository;
            _logger = logger;
        }

        // Shows the public Hall of Fame.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users =
                (await _userRepository.GetAllUsersAsync())
                .Where(u => !u.IsAdmin)
                .ToList();

            var activeAchievements =
                (await _achievementRepository.GetActiveAsync())
                .ToList();

            // Goals do not contribute Achievement Points.
            int maxPoints =
                activeAchievements
                    .Where(a => !a.IsGoal)
                    .Sum(a => a.Points);

            var hallOfFameUsers =
                new List<HallOfFameUserViewModel>();

            foreach (var user in users)
            {
                var userAchievements =
                    (await _achievementRepository
                        .GetUserAchievementsAsync(user.UserId))
                    .Where(ua => ua.Achievement.IsActive)
                    .ToList();

                int achievementPoints =
                    userAchievements
                        .Where(ua => !ua.Achievement.IsGoal)
                        .Sum(ua => ua.Achievement.Points);

                var milestone =
                    GetMilestone(
                        achievementPoints,
                        maxPoints);

                hallOfFameUsers.Add(
                    new HallOfFameUserViewModel
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        MilestoneName = milestone.Name,
                        MilestoneIcon = milestone.Icon,
                        AchievementCount =
                            userAchievements.Count,
                        AchievementPoints =
                            achievementPoints
                    });
            }

            // Users with the most AP are shown first.
            hallOfFameUsers =
                hallOfFameUsers
                    .OrderByDescending(
                        u => u.AchievementPoints)
                    .ThenBy(u => u.Username)
                    .ToList();

            // Users with equal AP share the same rank.
            int rank = 0;
            int? previousPoints = null;

            foreach (var user in hallOfFameUsers)
            {
                if (previousPoints == null ||
                    user.AchievementPoints != previousPoints)
                {
                    rank++;
                }

                user.Rank = rank;

                previousPoints =
                    user.AchievementPoints;
            }

            var viewModel =
                new HallOfFameViewModel
                {
                    Users = hallOfFameUsers
                };

            _logger.LogInformation(
                "Hall of Fame viewed.");

            return View(viewModel);
        }

        // Shows the public achievement page for a user.
        [HttpGet]
        public async Task<IActionResult> Achievements(int id)
        {
            var user =
                await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Admins do not participate in achievements.
            if (user.IsAdmin)
            {
                return NotFound();
            }

            // Only published achievements are shown publicly.
            var achievements =
                (await _achievementRepository.GetActiveAsync())
                .OrderBy(a => GetAchievementCategoryOrder(a))
                .ThenBy(a => a.RequirementValue)
                .ThenBy(a => a.AchievementId)
                .ToList();

            var userAchievements =
                (await _achievementRepository
                    .GetUserAchievementsAsync(user.UserId))
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
                    Username = user.Username,
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
                        Name =
                            achievement.Name,
                        Description =
                            achievement.Description,
                        Points =
                            achievement.Points,
                        IsGoal =
                            achievement.IsGoal,
                        IsUnlocked =
                            unlocked != null,
                        UnlockedAt =
                            unlocked?.UnlockedAt
                    });
            }

            SetMilestone(viewModel);

            _logger.LogInformation(
                "Public achievements viewed for user {UserId}.",
                user.UserId);

            return View(
                "~/Views/Achievement/Index.cshtml",
                viewModel);
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

        // Returns the milestone for a given AP total.
        private static (string Name, string Icon) GetMilestone(
            int totalPoints,
            int maxPoints)
        {
            if (maxPoints <= 0)
            {
                return (
                    "Getting Started",
                    "🌱");
            }

            int bronze =
                (int)Math.Ceiling(maxPoints * 0.20);

            int silver =
                (int)Math.Ceiling(maxPoints * 0.40);

            int gold =
                (int)Math.Ceiling(maxPoints * 0.60);

            int diamond =
                (int)Math.Ceiling(maxPoints * 0.80);

            if (totalPoints >= maxPoints)
            {
                return (
                    "Legend",
                    "👑");
            }

            if (totalPoints >= diamond)
            {
                return (
                    "Diamond",
                    "💎");
            }

            if (totalPoints >= gold)
            {
                return (
                    "Gold",
                    "🥇");
            }

            if (totalPoints >= silver)
            {
                return (
                    "Silver",
                    "🥈");
            }

            if (totalPoints >= bronze)
            {
                return (
                    "Bronze",
                    "🥉");
            }

            return (
                "Getting Started",
                "🌱");
        }

        // Calculates the milestone and progress
        // from the currently available Achievement Points.
        private static void SetMilestone(
            AchievementsViewModel viewModel)
        {
            int maxPoints =
                viewModel.MaxPoints;

            int totalPoints =
                viewModel.TotalPoints;

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