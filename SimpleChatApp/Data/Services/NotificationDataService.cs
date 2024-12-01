using Microsoft.EntityFrameworkCore;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    public class NotificationDataService : INotificationDataService
    {
        AppDbContext _context;
        public NotificationDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<InviteNotification?> AddInviteNotificationAsync(InviteNotification notification)
        {
            var existingEntry = await _context.InviteNotifications
                .FirstOrDefaultAsync(e => e.TargetId == notification.TargetId
                                     && e.ChatRoomName == notification.ChatRoomName);
            if (existingEntry != null)
                return null;

            _context.InviteNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<InviteNotification>> GetInviteNotifications(string userId)
        {
            var notifications = await _context.InviteNotifications
                .Where(n => n.TargetId == userId)
                .ToListAsync();

            return notifications ?? new List<InviteNotification>();
        }

        public async Task<InviteNotification?> RemoveInviteNotificationAsync(InviteNotification notification)
        {
            _context.InviteNotifications.Remove(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
    }
}
