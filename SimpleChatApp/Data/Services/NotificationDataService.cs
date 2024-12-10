using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
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
