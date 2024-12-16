using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SimpleChatApp.Data.Services;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models.Notifications;
using System.Security.Claims;
using System.Text.Json;

namespace SimpleChatApp.Hubs
{
    [Authorize]
    public class AppHub : Hub<IHubClient>
    {
        IChatDataService _chatDataService;
        INotificationDataService _notificationDataService;
        IMessageDataService _messageDataService;
        UserManager<User> _userManager;
        IUserHubContextManager _userHubContextManager;
        IInvitationService _invitationService;

        public AppHub(IChatDataService chatDataService,
            INotificationDataService notificationDataService,
            IMessageDataService messageDataService,
            UserManager<User> userManager,
            IUserHubContextManager userHubContextManager,
            IInvitationService invitationService)
        {
            _chatDataService = chatDataService;
            _notificationDataService = notificationDataService;
            _messageDataService = messageDataService;
            _userManager = userManager;
            _userHubContextManager = userHubContextManager;
            _invitationService = invitationService;
        }

        public override async Task OnConnectedAsync()
        {
            _userHubContextManager.AddUserHubContext(Context.UserIdentifier!,
                new HubCallerContextWrapper(Context));

            string userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userChats = await _chatDataService.GetUserChatsAsync(userId);
            foreach (var chat in userChats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Name);
            }

            var invitations = await _notificationDataService.GetInviteNotifications(Context.UserIdentifier!);

            foreach (var invitation in invitations) // TODO: change bunch notifications sending? ...
            {
                await Clients.Caller.OnInvited(invitation.SourceUserName, invitation.ChatRoomName);
            }

            await base.OnConnectedAsync();
            return;
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _userHubContextManager.RemoveUserHubContext(Context.UserIdentifier!,
                new HubCallerContextWrapper(Context));

            return base.OnDisconnectedAsync(exception);
        }

        
    }
}
