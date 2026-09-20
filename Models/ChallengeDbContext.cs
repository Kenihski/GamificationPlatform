using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Models
{
    public class ChallengeDbContext : DbContext
    {
        public ChallengeDbContext(
            DbContextOptions<ChallengeDbContext> options)
            : base(options)
        {
            Database.EnsureCreated(); // For early prototyping only. Remove when switing to EF Core Migrations.
        }

        public DbSet<Challenge> Challenges { get; set; }
    }
}