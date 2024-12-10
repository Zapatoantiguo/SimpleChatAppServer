using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models;
using SimpleChatApp.ErrorHandling.ResultPattern;

namespace SimpleChatApp.Data.Services
{
    public interface IChatDataService
    {
        public Task<Result<ChatRoomDto>> CreateChatAsync(string creatorId, ChatRoomDto chatDto);
        public Task<Result<User>> AddUserToChatAsync(string userId, string chatRoomName);
        public Task<Result<User>> RemoveUserFromChatAsync(string userId, string chatRoomName);
        public Task<Result<List<UserDto>>> GetChatMembersAsync(string requesterId, string chatRoomName);
        public Task<Result<int>> GetChatIdByName(string chatRoomName);
        public Task<bool> IsUserInChat(string userId, string chatRoomName);
        public Task<List<ChatRoomDto>> GetUserChatsAsync(string userId);
    }
}
