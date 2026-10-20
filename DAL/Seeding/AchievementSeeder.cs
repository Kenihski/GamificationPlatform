
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public static class AchievementSeeder
    {
        public static void Seed(ChallengeDbContext context)
        {
            // --------------------------------
            // Achievements
            // --------------------------------
            var achievements = new List<Achievement>
            {
                // --------------------------------
                // Challenge Completion
                // --------------------------------
                new Achievement
                {
                    Name = "First Challenge",
                    Description =
                        "Complete your first challenge.",
                    Points = 10,
                    Type = "ChallengesCompleted",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Getting the Hang of It",
                    Description =
                        "Complete 5 challenges.",
                    Points = 10,
                    Type = "ChallengesCompleted",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Challenge Hunter",
                    Description =
                        "Complete 10 challenges.",
                    Points = 15,
                    Type = "ChallengesCompleted",
                    RequirementValue = 10,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Dedicated Learner",
                    Description =
                        "Complete 20 challenges.",
                    Points = 20,
                    Type = "ChallengesCompleted",
                    RequirementValue = 20,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Score
                // --------------------------------
                new Achievement
                {
                    Name = "Halfway There",
                    Description =
                        "Score at least 50% on a challenge.",
                    Points = 5,
                    Type = "ScorePercentage",
                    RequirementValue = 50,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Getting Better",
                    Description =
                        "Score at least 60% on a challenge.",
                    Points = 5,
                    Type = "ScorePercentage",
                    RequirementValue = 60,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Good Job!",
                    Description =
                        "Score at least 70% on a challenge.",
                    Points = 10,
                    Type = "ScorePercentage",
                    RequirementValue = 70,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Great Job!",
                    Description =
                        "Score at least 80% on a challenge.",
                    Points = 10,
                    Type = "ScorePercentage",
                    RequirementValue = 80,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Almost Perfect",
                    Description =
                        "Score at least 90% on a challenge.",
                    Points = 15,
                    Type = "ScorePercentage",
                    RequirementValue = 90,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Perfect Score",
                    Description =
                        "Score 100% on a challenge.",
                    Points = 20,
                    Type = "ScorePercentage",
                    RequirementValue = 100,
                    IsActive = true
                },

                // --------------------------------
                // Practice
                // --------------------------------
                new Achievement
                {
                    Name = "Second Time's the Charm",
                    Description =
                        "Improve your score on your second attempt.",
                    Points = 10,
                    Type = "SecondAttemptImprovement",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Practice Makes Perfect",
                    Description =
                        "Complete the same challenge 5 times.",
                    Points = 15,
                    Type = "SameChallengeAttempts",
                    RequirementValue = 5,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Creator
                // --------------------------------
                new Achievement
                {
                    Name = "Challenge Creator",
                    Description =
                        "Create your first challenge.",
                    Points = 5,
                    Type = "ChallengesCreated",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Content Creator",
                    Description =
                        "Create 5 challenges.",
                    Points = 15,
                    Type = "ChallengesCreated",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "First Participant",
                    Description =
                        "Have another user complete one of your challenges.",
                    Points = 10,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 1,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Popular Creator",
                    Description =
                        "Have 5 different users complete challenges you created.",
                    Points = 10,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 5,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Community Favorite",
                    Description =
                        "Have 10 different users complete challenges you created.",
                    Points = 15,
                    Type = "CreatorUniqueParticipants",
                    RequirementValue = 10,
                    IsActive = true
                },

                // --------------------------------
                // Challenge Leaderboard
                // --------------------------------
                new Achievement
                {
                    Name = "So Close, Yet So Far",
                    Description =
                        "Reach 4th place on a Challenge Leaderboard.",
                    Points = 5,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 4,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "On the Podium",
                    Description =
                        "Reach 3rd place on a Challenge Leaderboard.",
                    Points = 10,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 3,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Silver Medalist",
                    Description =
                        "Reach 2nd place on a Challenge Leaderboard.",
                    Points = 15,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 2,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Challenge Champion",
                    Description =
                        "Reach 1st place on a Challenge Leaderboard.",
                    Points = 20,
                    Type = "ChallengeLeaderboardPosition",
                    RequirementValue = 1,
                    IsActive = true
                },

                // --------------------------------
                // Core Leaderboard
                // --------------------------------
                new Achievement
                {
                    Name = "Core Contender",
                    Description =
                        "Reach 4th place on the Core Leaderboard.",
                    Points = 5,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 4,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Bronze",
                    Description =
                        "Reach 3rd place on the Core Leaderboard.",
                    Points = 10,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 3,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Silver",
                    Description =
                        "Reach 2nd place on the Core Leaderboard.",
                    Points = 15,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 2,
                    IsActive = true
                },
                new Achievement
                {
                    Name = "Core Champion",
                    Description =
                        "Reach 1st place on the Core Leaderboard.",
                    Points = 20,
                    Type = "CoreLeaderboardPosition",
                    RequirementValue = 1,
                    IsActive = true
                },

                // --------------------------------
                // Goals
                // --------------------------------
                new Achievement
                {
                    Name = "Bronze Achiever",
                    Description =
                        "Reach the Bronze milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 20,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Silver Achiever",
                    Description =
                        "Reach the Silver milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 40,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Gold Achiever",
                    Description =
                        "Reach the Gold milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 60,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Diamond Achiever",
                    Description =
                        "Reach the Diamond milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 80,
                    IsActive = true,
                    IsGoal = true
                },
                new Achievement
                {
                    Name = "Legendary",
                    Description =
                        "Reach the Legend milestone.",
                    Points = 0,
                    Type = "Milestone",
                    RequirementValue = 100,
                    IsActive = true,
                    IsGoal = true
                }
            };

            context.Achievements.AddRange(achievements);
            context.SaveChanges();
        }
    }
}
