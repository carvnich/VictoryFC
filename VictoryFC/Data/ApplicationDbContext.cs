using Microsoft.EntityFrameworkCore;
using VictoryFC.Models;

namespace VictoryFC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Scorer> Scorers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indexes for performance only - no relationships
            modelBuilder.Entity<Match>()
                .HasIndex(m => new { m.Competition, m.Date });

            modelBuilder.Entity<Scorer>()
                .HasIndex(s => new { s.Competition, s.Goals });
        }
    }
}