namespace OddsCollector.Data
{
    using Microsoft.EntityFrameworkCore;
    using Models;

    /// <summary>
    /// Application database context.
    /// </summary>
    public class ApplicationDatabaseContext : DbContext
    {
        /// <summary>
        /// Odds table.
        /// </summary>
        public DbSet<Odd>? Odds { get; set; }

        /// <summary>
        /// Table with sport events.
        /// </summary>
        public DbSet<SportEvent>? SportEvents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDatabaseContext" /> class using the specified options.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types exposed in <see cref="DbSet{TEntity}" /> properties.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // the only way to declare a compound primary key
            modelBuilder.Entity<Odd>().HasKey(o => new { o.SportEventId, o.LastUpdate, o.Bookmaker });
        }
    }
}