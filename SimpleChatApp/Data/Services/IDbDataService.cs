using SimpleChatApp.Models;
using SimpleChatApp.Models.DTO;

namespace SimpleChatApp.Data.Services
{
    public interface IDbDataService
    {
        public Task<ChatRoomDto> CreateChatAsync(User creator, ChatRoomDto chatDto);
        public Task<List<UserDto>> GetUsersByPatternAsync(UserSearchDto pattern);
        
        public Task<UserProfileDto> CreateUserProfileAsync(User user);
        public Task<UserProfileDto> UpdateUserProfileAsync(User user, UserProfileDto profile);
        public Task<UserProfileDto?> GetUserProfileAsync(string userId);
        public Task<FriendDto?> AddFriendAsync(User user, FriendDto friend);
        public Task<FriendDto?> RemoveFriendAsync(User user, FriendDto friend);
        public Task<List<FriendDto>> GetAllFriendsAsync(User user);
        public Task<List<ChatRoomDto>> GetUserChats(User user);
        public Task<List<UserDto>?> GetChatMembers(User user, string chatRoomName);
        public Task<List<MessageDto>?> GetLastMessages(User user, string chatRoomName, uint pageNumber, uint pageSize);

    }
}
