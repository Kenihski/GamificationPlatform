using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public class ChallengeDbContext : DbContext
    {
        public ChallengeDbContext(
            DbContextOptions<ChallengeDbContext> options)
            : base(options)
        {
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

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // SQLite NOCASE makes username lookup and uniqueness consistent
            // for ASCII case variants such as Alice and alice.
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .UseCollation("NOCASE");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // One user can create many challenges.
            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CreatedChallenges)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
