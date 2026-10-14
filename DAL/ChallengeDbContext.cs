
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

namespace GamificationPlatform.DAL
{
    public class ChallengeDbContext
        : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ChallengeDbContext(
            DbContextOptions<ChallengeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Challenge> Challenges { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionOption> QuestionOptions { get; set; }

        public DbSet<QuestionAcceptedAnswer> QuestionAcceptedAnswers { get; set; }

        public DbSet<UserChallenge> UserChallenges { get; set; }

        public DbSet<ChallengeAttempt> ChallengeAttempts { get; set; }

        public DbSet<AttemptAnswer> AttemptAnswers { get; set; }

        public DbSet<AttemptAnswerOption> AttemptAnswerOptions { get; set; }

        public DbSet<Achievement> Achievements { get; set; }

        public DbSet<UserAchievement> UserAchievements { get; set; }

        public DbSet<UserImage> UserImages { get; set; }

        public DbSet<DefaultImage> DefaultImages { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // Configure ASP.NET Core Identity tables and relationships.
            base.OnModelCreating(modelBuilder);

            // SQLite NOCASE makes username lookup consistent
            // for ASCII case variants such as Alice and alice.
            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .UseCollation("NOCASE");

            // One user can create many challenges.
            modelBuilder.Entity<Challenge>()
                .HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CreatedChallenges)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One user can upload many images.
            modelBuilder.Entity<UserImage>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserImages)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One selected option can only be stored once
            // for each attempt answer.
            modelBuilder.Entity<AttemptAnswerOption>()
                .HasIndex(ao => new
                {
                    ao.AttemptAnswerId,
                    ao.QuestionOptionId
                })
                .IsUnique();

            // A user can only unlock each achievement once.
            modelBuilder.Entity<UserAchievement>()
                .HasIndex(ua => new
                {
                    ua.UserId,
                    ua.AchievementId
                })
                .IsUnique();

            // Delete achievement unlock records when
            // the related achievement is deleted.
            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
