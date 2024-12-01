using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    public interface IDbDataService
    {
        public Task<ChatRoomDto> CreateChatAsync(User creator, ChatRoomDto chatDto);
        public Task<User?> AddUserToChatAsync(string userId, string chatRoomName);
        public Task<User?> RemoveUserFromChatAsync(string userId, string chatRoomName);
        public Task<List<UserDto>> GetUsersByPatternAsync(UserSearchDto pattern);
        public Task<User?> GetUserByNameAsync(string userName);
        public Task<UserProfileDto> CreateUserProfileAsync(User user);
        public Task<UserProfileDto> UpdateUserProfileAsync(User user, UserProfileDto profile);
        public Task<UserProfileDto?> GetUserProfileAsync(string userId);
        public Task<FriendDto?> AddFriendAsync(User user, FriendDto friend);
        public Task<FriendDto?> RemoveFriendAsync(User user, FriendDto friend);
        public Task<List<FriendDto>> GetAllFriendsAsync(User user);
        public Task<List<ChatRoomDto>> GetUserChatsAsync(User user);
        public Task<List<UserDto>?> GetChatMembersAsync(User user, string chatRoomName);
        public Task<List<MessageDto>?> GetLastMessagesAsync(User user, string chatRoomName, uint pageNumber, uint pageSize);
        public Task<bool> IsUserExist(string userName);
        public Task<int?> GetChatIdByName(string chatRoomName);
        public Task<bool> IsUserInChat(string userId, string chatRoomName);
        public Task<InviteNotification?> AddInviteNotificationAsync(InviteNotification notification);
        public Task<InviteNotification?> RemoveInviteNotificationAsync(InviteNotification notification);
        public Task<List<InviteNotification>> GetInviteNotifications(string userId);
        public Task<Message?> AddMessageAsync(Message message);
    }
}
