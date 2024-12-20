using Microsoft.EntityFrameworkCore;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_DAL.Models;
using SimpleChatApp_DAL.Models.Notifications;
using SimpleChatApp_DAL;

namespace SimpleChatApp_BAL.Services
{
    public class NotificationDataService : INotificationDataService
    {
        AppDbContext _context;
        IChatDataService _chatDataService;
        public NotificationDataService(AppDbContext context,
            IChatDataService chatDataService)
        {
            _context = context;
            _chatDataService = chatDataService;
        }
        public async Task<Result<InviteNotification>> AddInviteNotificationAsync(InviteNotification notification)
        {
            var entryExists = await _context.InviteNotifications
                .AnyAsync(e => e.TargetId == notification.TargetId && e.ChatRoomName == notification.ChatRoomName);

            if (entryExists)
                return Result<InviteNotification>.Failure(NotificationErrors.InvitationAlreadyExists());

            _context.InviteNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return Result<InviteNotification>.Success(notification);
        }

        public async Task<List<InviteNotification>> GetInviteNotifications(string userId)
        {
            var notifications = await _context.InviteNotifications
                .Where(n => n.TargetId == userId)
                .ToListAsync();

            return notifications ?? new List<InviteNotification>();
        }     
    }
}
