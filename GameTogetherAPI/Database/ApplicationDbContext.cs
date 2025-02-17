using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Database {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Define Many-to-Many relationship between Games and Users
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Users)
                .WithMany(u => u.Games)
                .UsingEntity(j => j.ToTable("GamesWithUsers"));

            base.OnModelCreating(modelBuilder);
        }

    }
}
