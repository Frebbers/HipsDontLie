using GameTogetherAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTogetherAPI.Database {
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSession>()
                .HasKey(us => new { us.UserId, us.SessionId });

            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany(u => u.JoinedSessions)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(us => us.SessionId);
        }
    }

}
