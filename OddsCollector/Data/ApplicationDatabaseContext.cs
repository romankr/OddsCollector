namespace OddsCollector.Data
{
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class ApplicationDatabaseContext : DbContext
    {
        public DbSet<Odd>? Odds { get; set; }

        public DbSet<SportEvent>? SportEvents { get; set; }

        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Odd>().HasKey(o => new { o.SportEventId, o.LastUpdate, o.Bookmaker });
        }
    }
}