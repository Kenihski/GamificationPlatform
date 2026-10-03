using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to questions.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;
        private readonly ILogger<QuestionRepository> _logger;

        public QuestionRepository(
            ChallengeDbContext challengeDbContext,
            ILogger<QuestionRepository> logger)
        {
            _challengeDbContext = challengeDbContext;
            _logger = logger;
        }

        // Returns all questions together with their challenge.
        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            try
            {
                return await _challengeDbContext.Questions
                    .Include(q => q.Challenge)
                    .ToListAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(GetAllQuestionsAsync),
                    nameof(QuestionRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a question by its id.
        public async Task<Question?> GetQuestionByIdAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Questions
                    .FirstOrDefaultAsync(q =>
                        q.QuestionId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (QuestionId {QuestionId}).",
                    nameof(GetQuestionByIdAsync),
                    nameof(QuestionRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a question together with its answer options.
        public async Task<Question?> GetQuestionWithOptionsAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Questions
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q =>
                        q.QuestionId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (QuestionId {QuestionId}).",
                    nameof(GetQuestionWithOptionsAsync),
                    nameof(QuestionRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Returns a question together with its answer options and challenge.
        public async Task<Question?> GetQuestionWithOptionsAndChallengeAsync(
            int id)
        {
            try
            {
                return await _challengeDbContext.Questions
                    .Include(q => q.Options)
                    .Include(q => q.Challenge)
                    .FirstOrDefaultAsync(q =>
                        q.QuestionId == id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository} (QuestionId {QuestionId}).",
                    nameof(GetQuestionWithOptionsAndChallengeAsync),
                    nameof(QuestionRepository),
                    id);

                // Preserve the failure and its original stack trace.
                throw;
            }
        }

        // Creates a new question.
        public async Task<bool> CreateQuestionAsync(
            Question question)
        {
            _challengeDbContext.Questions.Add(question);

            return await SaveChangesAsync();
        }

        // Updates an existing question.
        public async Task<bool> UpdateQuestionAsync(
            Question question)
        {
            _challengeDbContext.Questions.Update(question);

            return await SaveChangesAsync();
        }

        // Deletes an existing question.
        public async Task<bool> DeleteQuestionAsync(
            Question question)
        {
            _challengeDbContext.Questions.Remove(question);

            return await SaveChangesAsync();
        }

        // Saves pending database changes.
        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _challengeDbContext
                    .SaveChangesAsync() > 0;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Database operation {Operation} failed in {Repository}.",
                    nameof(SaveChangesAsync),
                    nameof(QuestionRepository));

                // Preserve the failure and its original stack trace.
                throw;
            }
        }
    }
}
