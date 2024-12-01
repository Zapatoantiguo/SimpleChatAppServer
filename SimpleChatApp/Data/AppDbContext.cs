﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<UserChatRoom> UserChatRoom { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<InviteNotification> InviteNotifications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany();

            builder.Entity<User>()
                .HasMany(u => u.ChatRooms)
                .WithMany(chat => chat.Users)
                .UsingEntity<UserChatRoom>();

            builder.Entity<Message>()
                .HasOne(msg => msg.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserProfile>()
                .HasIndex(up => up.Nickname)    // can't find a better way to make field unique yet
                .IsUnique();

            builder.Entity<InviteNotification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey("TargetId")
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }

    }
}
