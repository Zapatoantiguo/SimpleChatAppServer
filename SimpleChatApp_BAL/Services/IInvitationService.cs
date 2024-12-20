using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_DAL.Models.Notifications;

namespace SimpleChatApp_BAL.Services
{
    public interface IInvitationService
    {
        public Task<Result<InviteNotification>> HandleInviteRequestAsync(
            string sourceId, string targetUserName, string chatRoomName);
        public Task<Result<InviteNotification>> HandleInviteRespondAsync(string userId, string chatRoomName, bool accept);
    }
}
