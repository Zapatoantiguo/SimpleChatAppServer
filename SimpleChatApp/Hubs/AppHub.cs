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
            _userHubContextManager.AddUserHubContext(Context.UserIdentifier!, Context);

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
            _userHubContextManager.RemoveUserHubContexts(Context.UserIdentifier!);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<int> InviteToChatRoom(string targetUserName, string chatRoomName)
        {
            var invResult = await _invitationService.HandleInviteRequestAsync(Context.UserIdentifier!, targetUserName, chatRoomName);
            if (invResult.IsFailure)
                return -1;  // TODO: make a decision on hub methods return types (are ProblemDetails acceptable?)

            var invitation = invResult.Value;
            // Send invitation to a target if one is connected
            if (Clients.User(invitation.TargetId) is not null)
            {
                await Clients.User(invitation.TargetId).OnInvited(invitation.SourceUserName, chatRoomName);
            }

            return 0;
        }
        public async Task<int> RespondToInvite(string chatRoomName, bool accept)
        {
            var invHandleResult = await _invitationService
                .HandleInviteRespondAsync(Context.UserIdentifier!, chatRoomName, accept);

            if (invHandleResult.IsFailure)
                return -1;

            if (accept)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomName);

                // add current and other connections of this user to hub group
                List<string>? userConnections = _userHubContextManager.GetUserConnectionIds(Context.UserIdentifier!);
                foreach (var connectionId in userConnections!)
                    await Groups.AddToGroupAsync(connectionId, chatRoomName);

                string userName = Context.User!.FindFirstValue(ClaimTypes.Name)!;
                await Clients.OthersInGroup(chatRoomName).OnUserJoinedChat(userName, chatRoomName);
            }
            
            return 0;
        }
        public async Task<int> SendMessage(string chatRoomName, string message)
        {
            string senderId = Context.UserIdentifier!;        

            var addMsgResult = await _messageDataService.AddMessageAsync(senderId, chatRoomName, message);

            if (addMsgResult.IsFailure) 
                return -1;

            var msg = addMsgResult.Value;
            MessageDto msgDto = new()
            {
                AuthorAlias = msg.AuthorAlias,
                Content = msg.Content,
                SentAt = msg.SentAt
            };
            await Clients.Group(chatRoomName).OnMessageReceived(msgDto);
            return 0;
        }

        public async Task<int> LeaveChat(string chatRoomName)
        {
            var userIsInChat = await _chatDataService.IsUserInChat(Context.UserIdentifier!, chatRoomName);
            if (!userIsInChat)
                return -1;  // caller is not in chat

            User caller = (await _userManager.GetUserAsync(Context.User!))!;

            var removeResult = await _chatDataService.RemoveUserFromChatAsync(caller.Id, chatRoomName);
            if (removeResult.IsFailure)
                return -2;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomName);
            await Clients.Group(chatRoomName).OnUserLeavedChat(caller.UserName!, chatRoomName);

            return 0;
        }
    }
}
