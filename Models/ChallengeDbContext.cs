using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Models
{
    public class ChallengeDbContext : DbContext
    {
        public ChallengeDbContext(
            DbContextOptions<ChallengeDbContext> options)
            : base(options)
        {
            // Database.EnsureCreated(); // For early prototyping only. Remove when switching to EF Core Migrations.
        }

        public DbSet<Challenge> Challenges { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionOption> QuestionOptions { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserChallenge> UserChallenges { get; set; }

        public DbSet<ChallengeAttempt> ChallengeAttempts { get; set; }

        public DbSet<AttemptAnswer> AttemptAnswers { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}