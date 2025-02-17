using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Database {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Games)
                .WithOne(g => g.Owner)
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Cascade); // This will delete games if user is deleted

            base.OnModelCreating(modelBuilder);
        }
    }
}
