using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    // Defines the database operations available for questions.
    // This interface separates the controllers from the database implementation.
    public interface IQuestionRepository
    {
        // Returns all questions together with their challenge.
        Task<List<Question>> GetAllQuestionsAsync();

        // Returns a question by its id.
        Task<Question?> GetQuestionByIdAsync(
            int id);

        // Returns a question together with its answer options.
        Task<Question?> GetQuestionWithOptionsAsync(
            int id);

        // Returns a question together with its answer options and challenge.
        Task<Question?> GetQuestionWithOptionsAndChallengeAsync(
            int id);

        // Creates a new question.
        Task<bool> CreateQuestionAsync(
            Question question);

        // Updates an existing question.
        Task<bool> UpdateQuestionAsync(
            Question question);

        // Deletes an existing question.
        Task<bool> DeleteQuestionAsync(
            Question question);

        // Saves pending changes to the database.
        Task<bool> SaveChangesAsync();
    }
}