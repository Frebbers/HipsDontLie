﻿using HipsDontLie.Models;
using Microsoft.EntityFrameworkCore;

namespace HipsDontLie.Database {
    /// <summary>
    /// Represents the database context for the application, handling entity configurations and database interactions.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class with the specified database options.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Represents the users in the database.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Represents user profiles associated with users in the database.
        /// </summary>
        public DbSet<Profile> Profiles { get; set; }

        /// <summary>
        /// Represents groups available in the database.
        /// </summary>
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Represents the many-to-many relationship between users and groups.
        /// </summary>
        public DbSet<UserGroup> UserGroups { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserChat> UserChats { get; set; }

        /// <summary>
        /// Configures entity relationships and constraints using the Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to define entity relationships.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //If user deletes their user, their profile will also be deleted
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            //Many-to-many relationship between User and Groups
            modelBuilder.Entity<UserGroup>()
                .HasOne(us => us.User)
                .WithMany(u => u.JoinedGroups)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasKey(us => new { us.UserId, us.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(us => us.Group)
                .WithMany(s => s.Members)
                .HasForeignKey(us => us.GroupId);

            //Many-to-many relationship between User and Chat
            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(uc => uc.UserId);
            
            modelBuilder.Entity<UserChat>()
                .HasKey(uc => new { uc.UserId, uc.ChatId });

            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.Chat)
                .WithMany(c => c.UserChats)
                .HasForeignKey(uc => uc.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            //One-to-one "optional" relationship between Groups and Chat
            //A group may have a chat, but a chat can also exist without a group (private chat)
            modelBuilder.Entity<Group>()
                .HasOne(s => s.Chat)
                .WithOne(c => c.Group)
                .HasForeignKey<Chat>(c => c.GroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            //One-to-many relationship between Chat and Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                //If a chat is deleted, all the messages will also be deleted.
                .OnDelete(DeleteBehavior.Cascade);

            //One-to-many relationship between User and Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                //This one could change, at the moment it just sets the sender to null but perserves the messages.
                //Maybe the user should have the power to have their messages deleted if they delete their user (Cascade)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
