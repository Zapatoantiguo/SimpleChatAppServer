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

        public async Task<Result<InviteNotification>> HandleInviteRespondAsync(string userId, string chatRoomName, bool accept)
        {
            User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User ID {userId} doesn't exist in DB");

            ChatRoom? chat = await _context.ChatRooms.SingleOrDefaultAsync(c => c.Name == chatRoomName);
            if (chat == null)
                return Result<InviteNotification>.Failure(ChatErrors.NotFound(chatRoomName));

            var invitation = await _context.InviteNotifications
                .SingleOrDefaultAsync(n => n.ChatRoomName == chatRoomName && n.TargetId == userId);
            if (invitation == null)
                return Result<InviteNotification>.Failure(NotificationErrors.NotFound());

            if (accept)
            {
                // TODO: remove interservice dependency?
                var addResult = await _chatDataService.AddUserToChatAsync(invitation.TargetId, chatRoomName);
                if (addResult.IsFailure)
                    return Result<InviteNotification>.Failure(addResult.Error);
            }

            _context.InviteNotifications.Remove(invitation);
            await _context.SaveChangesAsync();

            return Result<InviteNotification>.Success(invitation);
        }
    }
}
