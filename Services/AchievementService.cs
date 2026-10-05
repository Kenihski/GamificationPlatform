using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly ChallengeDbContext _context;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(
            IAchievementRepository achievementRepository,
            ChallengeDbContext context,
            ILogger<AchievementService> logger)
        {
            _achievementRepository = achievementRepository;
            _context = context;
            _logger = logger;
        }

        public async Task CheckAchievementsAsync(int userId)
        {
            // Admins do not participate in the
            // achievement system or Hall of Fame.
            var user =
                await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.UserId == userId);

            if (user == null || user.IsAdmin)
            {
                return;
            }

            var achievements =
                (await _achievementRepository.GetActiveAsync())
                .ToList();

            // Check regular achievements first.
            foreach (var achievement in achievements.Where(a => !a.IsGoal))
            {
                if (await _achievementRepository.HasAchievementAsync(
                    userId,
                    achievement.AchievementId))
                {
                    continue;
                }

                bool unlocked = achievement.Type switch
                {
                    "ChallengesCompleted" =>
                        await CheckChallengesCompletedAsync(
                            userId,
                            achievement.RequirementValue),

                    "ScorePercentage" =>
                        await CheckScorePercentageAsync(
                            userId,
                            achievement.RequirementValue),

                    "SecondAttemptImprovement" =>
                        await CheckSecondAttemptImprovementAsync(userId),

                    "SameChallengeAttempts" =>
                        await CheckSameChallengeAttemptsAsync(
                            userId,
                            achievement.RequirementValue),

                    "ChallengesCreated" =>
                        await CheckChallengesCreatedAsync(
                            userId,
                            achievement.RequirementValue),

                    "CreatorUniqueParticipants" =>
                        await CheckCreatorUniqueParticipantsAsync(
                            userId,
                            achievement.RequirementValue),

                    "ChallengeLeaderboardPosition" =>
                        await CheckChallengeLeaderboardPositionAsync(
                            userId,
                            achievement.RequirementValue),

                    "CoreLeaderboardPosition" =>
                        await CheckCoreLeaderboardPositionAsync(
                            userId,
                            achievement.RequirementValue),

                    _ => false
                };

                if (unlocked)
                {
                    await UnlockAchievementAsync(
                        userId,
                        achievement);
                }
            }

            // Save regular achievements before checking goals,
            // because milestone goals depend on the user's AP.
            await _achievementRepository.SaveChangesAsync();

            // Check milestone goals after the AP has been updated.
            foreach (var achievement in achievements.Where(a => a.IsGoal))
            {
                if (await _achievementRepository.HasAchievementAsync(
                    userId,
                    achievement.AchievementId))
                {
                    continue;
                }

                bool unlocked =
                    await CheckMilestoneAsync(
                        userId,
                        achievement.RequirementValue);

                if (unlocked)
                {
                    await UnlockAchievementAsync(
                        userId,
                        achievement);
                }
            }

            await _achievementRepository.SaveChangesAsync();
        }

        private async Task<bool> CheckChallengesCompletedAsync(
            int userId,
            int requiredCount)
        {
            int completedChallenges =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.Completed)
                    .Select(a => a.UserChallenge.ChallengeId)
                    .Distinct()
                    .CountAsync();

            return completedChallenges >= requiredCount;
        }

        private async Task<bool> CheckScorePercentageAsync(
            int userId,
            int requiredPercentage)
        {
            var attempts =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.Completed)
                    .Select(a => new
                    {
                        a.Score,

                        // The maximum score is calculated
                        // from the questions in the challenge.
                        MaxPoints =
                            a.UserChallenge.Challenge.Questions
                                .Sum(q => q.Points)
                    })
                    .ToListAsync();

            return attempts.Any(a =>
                a.MaxPoints > 0 &&
                (double)a.Score / a.MaxPoints * 100
                    >= requiredPercentage);
        }

        private async Task<bool>
            CheckSecondAttemptImprovementAsync(int userId)
        {
            var attempts =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.Completed)
                    .OrderBy(a => a.CompletedAt)
                    .ThenBy(a => a.ChallengeAttemptId)
                    .Select(a => new
                    {
                        a.UserChallenge.ChallengeId,
                        a.Score
                    })
                    .ToListAsync();

            var challengeGroups =
                attempts.GroupBy(a => a.ChallengeId);

            foreach (var group in challengeGroups)
            {
                var firstTwo = group.Take(2).ToList();

                if (firstTwo.Count == 2 &&
                    firstTwo[1].Score > firstTwo[0].Score)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> CheckSameChallengeAttemptsAsync(
            int userId,
            int requiredCount)
        {
            // Get the number of completed attempts
            // for each challenge from the database.
            var attemptCounts =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.UserChallenge.UserId == userId &&
                        a.Completed)
                    .GroupBy(a =>
                        a.UserChallenge.ChallengeId)
                    .Select(group => group.Count())
                    .ToListAsync();

            // Find the challenge with the most attempts.
            int highestAttemptCount =
                attemptCounts.Count > 0
                    ? attemptCounts.Max()
                    : 0;

            return highestAttemptCount >= requiredCount;
        }

        private async Task<bool> CheckChallengesCreatedAsync(
            int userId,
            int requiredCount)
        {
            int createdChallenges =
                await _context.Challenges
                    .CountAsync(c =>
                        c.CreatedByUserId == userId);

            return createdChallenges >= requiredCount;
        }

        private async Task<bool>
            CheckCreatorUniqueParticipantsAsync(
                int userId,
                int requiredCount)
        {
            int uniqueParticipants =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.Completed &&
                        a.UserChallenge.Challenge.CreatedByUserId
                            == userId &&
                        a.UserChallenge.UserId != userId)
                    .Select(a => a.UserChallenge.UserId)
                    .Distinct()
                    .CountAsync();

            return uniqueParticipants >= requiredCount;
        }

        private async Task<bool>
            CheckChallengeLeaderboardPositionAsync(
                int userId,
                int requiredPosition)
        {
            // Get all challenges where the user has
            // at least one completed attempt.
            var challengeIds =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.Completed &&
                        a.UserChallenge.UserId == userId)
                    .Select(a =>
                        a.UserChallenge.ChallengeId)
                    .Distinct()
                    .ToListAsync();

            foreach (var challengeId in challengeIds)
            {
                var challenge =
                    await _context.Challenges
                        .FirstOrDefaultAsync(c =>
                            c.ChallengeId == challengeId);

                if (challenge == null)
                {
                    continue;
                }

                // The challenge creator is excluded
                // from their own leaderboard.
                if (challenge.CreatedByUserId == userId)
                {
                    continue;
                }

                var attempts =
                    await _context.ChallengeAttempts
                        .Where(a =>
                            a.Completed &&
                            a.UserChallenge.ChallengeId ==
                                challengeId &&
                            a.UserChallenge.UserId !=
                                challenge.CreatedByUserId)
                        .ToListAsync();

                // Only the first completed attempt
                // from each user can count.
                var firstAttempts =
                    attempts
                        .GroupBy(a =>
                            a.UserChallenge.UserId)
                        .Select(group =>
                            group
                                .OrderBy(a =>
                                    a.CompletedAt)
                                .ThenBy(a =>
                                    a.ChallengeAttemptId)
                                .First())
                        .Where(a =>
                            a.Score > 0)
                        .OrderByDescending(a =>
                            a.Score)
                        .ThenBy(a =>
                            a.CompletedAt!.Value -
                            a.StartedAt)
                        .ToList();

                int rank = 0;
                int? previousScore = null;
                TimeSpan? previousTime = null;

                foreach (var attempt in firstAttempts)
                {
                    TimeSpan timeUsed =
                        attempt.CompletedAt!.Value -
                        attempt.StartedAt;

                    // Dense ranking:
                    // equal score and time share the same rank.
                    if (previousScore == null ||
                        attempt.Score != previousScore ||
                        timeUsed != previousTime)
                    {
                        rank++;
                    }

                    // A better position also satisfies
                    // lower leaderboard achievements.
                    if (attempt.UserChallenge.UserId == userId &&
                        rank <= requiredPosition)
                    {
                        return true;
                    }

                    previousScore =
                        attempt.Score;

                    previousTime =
                        timeUsed;
                }
            }

            return false;
        }

        private async Task<bool>
            CheckCoreLeaderboardPositionAsync(
                int userId,
                int requiredPosition)
        {
            var attempts =
                await _context.ChallengeAttempts
                    .Where(a =>
                        a.Completed &&
                        a.UserChallenge.Challenge.IsCore)
                    .ToListAsync();

            // The challenge creator is excluded
            // from their own challenge.
            attempts =
                attempts
                    .Where(a =>
                        a.UserChallenge.UserId !=
                        a.UserChallenge.Challenge.CreatedByUserId)
                    .ToList();

            // Only the first completed attempt
            // from each user on each Core Challenge can count.
            var firstAttempts =
                attempts
                    .GroupBy(a => new
                    {
                        a.UserChallenge.UserId,
                        a.UserChallenge.ChallengeId
                    })
                    .Select(group =>
                        group
                            .OrderBy(a =>
                                a.CompletedAt)
                            .ThenBy(a =>
                                a.ChallengeAttemptId)
                            .First())
                    .Where(a =>
                        a.Score > 0)
                    .ToList();

            var entries =
                firstAttempts
                    .GroupBy(a =>
                        a.UserChallenge.UserId)
                    .Select(group =>
                    {
                        double totalPoints =
                            group.Sum(attempt =>
                            {
                                int maxPoints =
                                    attempt.UserChallenge
                                        .Challenge
                                        .Questions
                                        .Sum(q => q.Points);

                                if (maxPoints <= 0)
                                {
                                    return 0;
                                }

                                return
                                    (double)attempt.Score /
                                    maxPoints * 100;
                            });

                        TimeSpan totalTime =
                            TimeSpan.FromTicks(
                                group.Sum(attempt =>
                                    (attempt.CompletedAt!.Value -
                                     attempt.StartedAt).Ticks));

                        return new
                        {
                            UserId =
                                group.Key,

                            TotalPoints =
                                Math.Round(totalPoints, 1),

                            CoreChallengesCompleted =
                                group.Count(),

                            TotalTime =
                                totalTime
                        };
                    })
                    .OrderByDescending(entry =>
                        entry.TotalPoints)
                    .ThenBy(entry =>
                        entry.CoreChallengesCompleted)
                    .ThenBy(entry =>
                        entry.TotalTime)
                    .ToList();

            int rank = 0;
            double? previousPoints = null;
            int? previousChallenges = null;
            TimeSpan? previousTime = null;

            foreach (var entry in entries)
            {
                // Dense ranking:
                // completely equal results share the same rank.
                if (previousPoints == null ||
                    entry.TotalPoints != previousPoints ||
                    entry.CoreChallengesCompleted !=
                        previousChallenges ||
                    entry.TotalTime != previousTime)
                {
                    rank++;
                }

                // A better position also satisfies
                // lower leaderboard achievements.
                if (entry.UserId == userId &&
                    rank <= requiredPosition)
                {
                    return true;
                }

                previousPoints =
                    entry.TotalPoints;

                previousChallenges =
                    entry.CoreChallengesCompleted;

                previousTime =
                    entry.TotalTime;
            }

            return false;
        }

        private async Task<bool> CheckMilestoneAsync(
            int userId,
            int requiredPercentage)
        {
            // Only normal achievements contribute to AP.
            int maxPoints =
                await _context.Achievements
                    .Where(a =>
                        a.IsActive &&
                        !a.IsGoal)
                    .SumAsync(a => a.Points);

            if (maxPoints <= 0)
            {
                return false;
            }

            // Calculate the AP from achievements
            // the user has already unlocked.
            int userPoints =
                await _context.UserAchievements
                    .Where(ua =>
                        ua.UserId == userId &&
                        ua.Achievement.IsActive &&
                        !ua.Achievement.IsGoal)
                    .SumAsync(ua =>
                        ua.Achievement.Points);

            double percentage =
                (double)userPoints /
                maxPoints * 100;

            return percentage >= requiredPercentage;
        }

        private async Task UnlockAchievementAsync(
            int userId,
            Achievement achievement)
        {
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.AchievementId,
                UnlockedAt = DateTime.Now
            };

            await _achievementRepository
                .AddUserAchievementAsync(userAchievement);

            _logger.LogInformation(
                "User {UserId} unlocked achievement {AchievementName}.",
                userId,
                achievement.Name);
        }
    }
}