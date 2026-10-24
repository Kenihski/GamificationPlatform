
using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.DAL
{
    public static class AttemptSeeder
    {
        public static void Seed(ChallengeDbContext context)
        {
            var now = DateTime.Now;
            var samples = new (int UserId, int ChallengeId, int Correct, int DaysAgo, int Minutes)[]
            {
                (2,1,1,5,8), (2,1,2,3,6), (2,1,3,1,4),
                (3,1,3,2,7), (4,1,2,1,5), (5,1,1,1,6), (6,1,3,1,3),
                (2,2,2,3,8), (5,2,3,2,6), (6,2,1,1,9),
                (2,3,3,4,5), (3,3,2,2,7), (6,3,1,1,8),
                (2,4,3,2,9), (2,4,4,1,6), (3,4,4,1,8),
                (2,5,3,1,4), (4,5,2,1,5),
                (3,6,2,1,5), (5,6,3,1,3),
                (4,7,3,1,10), (6,7,4,1,7)
            };

            foreach (var sample in samples)
            {
                AddAttempt(context, sample.UserId, sample.ChallengeId,
                    sample.Correct, now.AddDays(-sample.DaysAgo), sample.Minutes);
            }
        }

        // CorrectAnswers is a question count, not a hardcoded score.
        public static void AddAttempt(
            ChallengeDbContext context, int userId, int challengeId,
            int correctAnswers, DateTime completedAt, int durationMinutes = 5)
        {
            var challenge = context.Challenges
                .Single(c => c.ChallengeId == challengeId);

            // Achievements and leaderboards should not count self-created quizzes.
            if (challenge.CreatedByUserId == userId)
            {
                return;
            }

            var questions = context.Questions
                .Include(q => q.Options)
                .Include(q => q.AcceptedAnswers)
                .Where(q => q.ChallengeId == challengeId)
                .OrderBy(q => q.QuestionId)
                .ToList();

            if (questions.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Challenge {challengeId} has no questions.");
            }

            correctAnswers = Math.Clamp(correctAnswers, 0, questions.Count);
            var userChallenge = context.UserChallenges
                .SingleOrDefault(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

            if (userChallenge == null)
            {
                userChallenge = new UserChallenge
                {
                    UserId = userId,
                    ChallengeId = challengeId
                };
                context.UserChallenges.Add(userChallenge);
                context.SaveChanges();
            }

            int score = 0;
            var attempt = new ChallengeAttempt
            {
                UserChallengeId = userChallenge.UserChallengeId,
                Completed = true,
                StartedAt = completedAt.AddMinutes(-durationMinutes),
                CompletedAt = completedAt
            };

            for (int index = 0; index < questions.Count; index++)
            {
                var question = questions[index];
                bool correct = index < correctAnswers;
                var answer = new AttemptAnswer { QuestionId = question.QuestionId };

                if (question.QuestionType == "ShortAnswer")
                {
                    if (correct && question.AcceptedAnswers.Count == 0)
                        throw new InvalidOperationException($"Missing accepted answer for question {question.QuestionId}.");

                    answer.TextAnswer = correct
                        ? question.AcceptedAnswers[0].Text
                        : "incorrect-demo-answer";
                }
                else
                {
                    var selected = correct
                        ? question.Options.Where(o => o.IsCorrect).ToList()
                        : question.Options.Where(o => !o.IsCorrect).Take(1).ToList();

                    if (correct && selected.Count == 0)
                        throw new InvalidOperationException($"Missing correct option for question {question.QuestionId}.");

                    // For multiple choice, a single wrong option must not accidentally
                    // match the complete correct option set.
                    if (!correct && selected.Count == 0)
                        throw new InvalidOperationException($"Missing incorrect option for question {question.QuestionId}.");

                    foreach (var option in selected)
                    {
                        answer.SelectedOptions.Add(new AttemptAnswerOption
                        {
                            QuestionOptionId = option.QuestionOptionId
                        });
                    }
                }

                if (correct) score += question.Points;
                attempt.Answers.Add(answer);
            }

            attempt.Score = score;
            context.ChallengeAttempts.Add(attempt);
            userChallenge.Score = score;
            userChallenge.Completed = true;
            context.SaveChanges();
        }
    }
}
