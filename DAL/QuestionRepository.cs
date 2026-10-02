using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Handles database operations related to questions.
    // Controllers use this repository instead of accessing
    // ChallengeDbContext directly.
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public QuestionRepository(
            ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        // Returns all questions together with their challenge.
        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await _challengeDbContext.Questions
                .Include(q => q.Challenge)
                .ToListAsync();
        }

        // Returns a question by its id.
        public async Task<Question?> GetQuestionByIdAsync(
            int id)
        {
            return await _challengeDbContext.Questions
                .FirstOrDefaultAsync(q =>
                    q.QuestionId == id);
        }

        // Returns a question together with its answer options.
        public async Task<Question?> GetQuestionWithOptionsAsync(
            int id)
        {
            return await _challengeDbContext.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q =>
                    q.QuestionId == id);
        }

        // Returns a question together with its answer options and challenge.
        public async Task<Question?> GetQuestionWithOptionsAndChallengeAsync(
            int id)
        {
            return await _challengeDbContext.Questions
                .Include(q => q.Options)
                .Include(q => q.Challenge)
                .FirstOrDefaultAsync(q =>
                    q.QuestionId == id);
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
            return await _challengeDbContext
                .SaveChangesAsync() > 0;
        }
    }
}