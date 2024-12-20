using SimpleChatApp_DAL.Models;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_BAL.DTO;

namespace SimpleChatApp_BAL.Services
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
