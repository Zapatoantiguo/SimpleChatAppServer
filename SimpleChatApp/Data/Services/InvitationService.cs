using Microsoft.EntityFrameworkCore;
using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    
    public class InvitationService : IInvitationService
    {
        readonly AppDbContext _context;
        readonly IChatDataService _chatDataService;
        readonly IUserDataService _userDataService;
        readonly INotificationDataService _notificationDataService;
        public InvitationService(AppDbContext appDbContext,
            IChatDataService chatDataService,
            IUserDataService userDataService,
            INotificationDataService notificationDataService)
        {
            _context = appDbContext;
            _chatDataService = chatDataService;
            _userDataService = userDataService;
            _notificationDataService = notificationDataService;
        }
        public async Task<Result<InviteNotification>> HandleInviteRequestAsync(
            string sourceId, string targetUserName, string chatRoomName)
        {
            User? caller = await _context.Users.SingleOrDefaultAsync(u => u.Id == sourceId);
            if (caller == null)
                throw new Exception($"User ID {sourceId} doesn't exist in DB");

            User? targetUser = await _context.Users
                .Include(u => u.Profile)
                .SingleOrDefaultAsync(u => u.UserName == targetUserName);
            if (targetUser == null)
                return Result<InviteNotification>.Failure(UserErrors.NotFound(targetUserName));

            var chat = await _context.ChatRooms
                .Where(ch => ch.Name == chatRoomName)
                .Include(ch => ch.UserChatRoom)
                .SingleOrDefaultAsync();

            if (chat == null)
                return Result<InviteNotification>.Failure(ChatErrors.NotFound(chatRoomName));

            if (!chat.UserChatRoom.Any(e => e.UserId == sourceId))  // source user not in chat
                return Result<InviteNotification>.Failure(ChatErrors.UserIsNotInChat());

            if (chat.UserChatRoom.Any(e => e.UserId == targetUser.Id))  // target in chat already
                return Result<InviteNotification>.Failure(ChatErrors.UserInChatAlready());

            var profile = targetUser.Profile;

            if (profile?.InventionOptions == ChatInventionOptions.FriendsOnly)
            {
                bool callerIsFriend = await _userDataService.CheckIsFriend(targetUser.Id, caller.Id);
                if (!callerIsFriend)
                    return Result<InviteNotification>.Failure(UserErrors.IsNotFriend());
            }
            else if (profile?.InventionOptions == ChatInventionOptions.ResidentsOnly)
            {
                if (caller.IsAnonimous)
                    return Result<InviteNotification>.Failure(Error.Failure(
                        "NotAllowed", "Target user doesn't accept invitations from anons"));
            }
            InviteNotification notification = new()
            {
                ChatRoomName = chatRoomName,
                SourceUserName = caller.UserName!,
                TargetUser = targetUser,
                TargetId = targetUser.Id
            };
            var addResult = await _notificationDataService.AddInviteNotificationAsync(notification);
            if (addResult.IsFailure)
                return Result<InviteNotification>.Failure(addResult.Error);

            return Result<InviteNotification>.Success(notification);
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
