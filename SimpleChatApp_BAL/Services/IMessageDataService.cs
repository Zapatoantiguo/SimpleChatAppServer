using SimpleChatApp_DAL.Models;
using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_BAL.DTO;

namespace SimpleChatApp_BAL.Services
{
    public interface IMessageDataService
    {
        public Task<Result<List<MessageDto>>> GetLastMessagesAsync(string userId, string chatRoomName, int pageNumber, int pageSize);
        public Task<Result<Message>> AddMessageAsync(string senderId, string chatName, string content);
    }
}
