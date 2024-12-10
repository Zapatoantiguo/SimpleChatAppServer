using SimpleChatApp.Models.DTO;
using SimpleChatApp.Models;
using SimpleChatApp.ErrorHandling.ResultPattern;

namespace SimpleChatApp.Data.Services
{
    public interface IMessageDataService
    {
        public Task<Result<List<MessageDto>>> GetLastMessagesAsync(string userId, string chatRoomName, int pageNumber, int pageSize);
        public Task<Result<Message>> AddMessageAsync(string senderId, string chatName, string content);
    }
}
