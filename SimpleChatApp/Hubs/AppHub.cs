using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SimpleChatApp.Data.Services;
using SimpleChatApp.Hubs.Services;
using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models.Notifications;
using System.Text.Json;

namespace SimpleChatApp.Hubs
{
    [Authorize]
    public class AppHub : Hub<IHubClient>
    {
        IUserDataService _userDataService;
        IChatDataService _chatDataService;
        INotificationDataService _notificationDataService;
        IMessageDataService _messageDataService;
        UserManager<User> _userManager;
        IUserHubContextManager _userHubContextManager;

        public AppHub(IUserDataService userDataService,
            IChatDataService chatDataService,
            INotificationDataService notificationDataService,
            IMessageDataService messageDataService,
            UserManager<User> userManager,
            IUserHubContextManager userHubContextManager)
        {
            _userDataService = userDataService;
            _chatDataService = chatDataService;
            _notificationDataService = notificationDataService;
            _messageDataService = messageDataService;
            _userManager = userManager;
            _userHubContextManager = userHubContextManager;
        }

        public override async Task OnConnectedAsync()
        {
            _userHubContextManager.AddUserHubContext(Context.UserIdentifier!, Context);

            User user = (await _userManager.GetUserAsync(Context.User!))!;

            var userChats = await _chatDataService.GetUserChatsAsync(user);
            foreach (var chat in userChats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Name);
            }

            var invitations = await _notificationDataService.GetInviteNotifications(Context.UserIdentifier!);
            if (invitations.Count > 0)
            {
                foreach (var invitation in invitations) // TODO: change bunch notifications sending ...
                {
                    await Clients.Caller.OnInvited(invitation.SourceUserName, invitation.ChatRoomName);
                }
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
            // check if such user and chat exist ...
            Task<User?> t1 = _userDataService.GetUserByNameAsync(targetUserName);
            Task<int?> t2 = _chatDataService.GetChatIdByName(chatRoomName);

            await Task.WhenAll(t1, t2);
            User? targetUser = t1.Result;
            if (targetUser == null)
                return -1;  // TODO: here and everywhere in hub: find a better solution for return statuses to clients
            if (t2.Result == null)
                return -2;
            
            // Register invitation: save notification with body in db ...
            User caller = await _userManager.GetUserAsync(Context.User);

            InviteNotification notification = new()
            {
                ChatRoomName = chatRoomName,
                SourceUserName = caller.UserName,
                TargetUser = targetUser
            };

            var addedNotification = await _notificationDataService.AddInviteNotificationAsync(notification);
            if (addedNotification == null)
                return -3;  // user is invited already

            // Call a method to send invitation to a target if one is connected
            if (Clients.User(targetUser.Id) is not null)
                await Clients.User(targetUser.Id).OnInvited(caller.UserName!, chatRoomName);

            return 0;
        }
        public async Task<int> RespondToInvite(string chatRoomName, bool accept)
        {
            List<InviteNotification> invites = await _notificationDataService.GetInviteNotifications(Context.UserIdentifier!);

            var invitation = invites.SingleOrDefault(n => n.ChatRoomName == chatRoomName);
            if (invitation == null)
                return -1;

            if (accept)
            {
                var addedUser = await _chatDataService.AddUserToChatAsync(invitation.TargetId, chatRoomName);

                // TODO: add processing of other current connections of this user 
                await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomName);
            }
            await _notificationDataService.RemoveInviteNotificationAsync(invitation);
            return 0;
        }
        public async Task<int> SendMessage(string chatRoomName, string message)
        {
            var userIsInChat = await _chatDataService.IsUserInChat(Context.UserIdentifier, chatRoomName);
            if (!userIsInChat)
                return -1;  // caller is not in chat

            User caller = await _userManager.GetUserAsync(Context.User);

            int chatId = (await _chatDataService.GetChatIdByName(chatRoomName)).Value;
            var msg = new Message
            {
                AuthorAlias = caller.UserName,
                Author = caller,
                Content = message,
                ChatRoomId = chatId,
                SentAt = DateTime.UtcNow
            };

            var addedMsg = await _messageDataService.AddMessageAsync(msg);


            MessageDto msgDto = new()
            {
                AuthorAlias = caller.UserName,
                Content = message,
                SentAt = msg.SentAt
            };
            await Clients.Group(chatRoomName).OnMessageReceived(msgDto);
            return 0;
        }

        public async Task<int> LeaveChat(string chatRoomName)
        {
            var userIsInChat = await _chatDataService.IsUserInChat(Context.UserIdentifier, chatRoomName);
            if (!userIsInChat)
                return -1;  // caller is not in chat

            User caller = await _userManager.GetUserAsync(Context.User);

            var removedUser = _chatDataService.RemoveUserFromChatAsync(caller.Id, chatRoomName);
            await Clients.Group(chatRoomName).OnUserLeavedChat(caller.UserName, chatRoomName);

            return 0;
        }
    }
}
