using Azure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<Friendship> Friendships => Set<Friendship>("Friendships");
        public DbSet<UserChatRoom> UserChatRoom { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<InviteNotification> InviteNotifications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(Console.WriteLine,
                    new[] { DbLoggerCategory.Query.Name },
                LogLevel.Information)
                .EnableSensitiveDataLogging();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);     // npgsql timestamps issue (https://www.npgsql.org/doc/types/datetime.html)

            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasMany(u => u.FriendsObjects)
                .WithMany(u => u.FriendsSubjects)
                .UsingEntity<Friendship>(
                    "Friendships",
                    l => l.HasOne<User>().WithMany().HasForeignKey("ObjectId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("SubjectId"),
                    k => k.HasKey("SubjectId", "ObjectId")
                    );

            builder.Entity<User>()
                .HasMany(u => u.ChatRooms)
                .WithMany(chat => chat.Users)
                .UsingEntity<UserChatRoom>();

            builder.Entity<Message>()
                .HasOne(msg => msg.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<UserProfile>()
                .HasIndex(up => up.Nickname)    // can't find a better way to make field unique yet
                .IsUnique();

            builder.Entity<InviteNotification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey("TargetId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoom>()
                .HasIndex(chat => chat.Name)
                .IsUnique();

            builder.Entity<UserChatRoom>()
                .Property(uc => uc.JoinedAt)
                .HasDefaultValueSql("now() at time zone 'utc'");

            base.OnModelCreating(builder);
        }

    }
}
