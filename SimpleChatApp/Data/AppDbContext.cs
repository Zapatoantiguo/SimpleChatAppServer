using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;

namespace SimpleChatApp.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<UserChatRoom> UserChatRoom { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
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
                .UsingEntity(typeof(UserChatRoom));

            builder.Entity<Message>()
                .HasOne(msg => msg.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);




            base.OnModelCreating(builder);
        }

    }
}
