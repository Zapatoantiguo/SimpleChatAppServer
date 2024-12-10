using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    public interface IInvitationService
    {
        public Task<Result<InviteNotification>> HandleInviteRequestAsync(
            string sourceId, string targetUserName, string chatRoomName);
        public Task<Result<InviteNotification>> HandleInviteRespondAsync(string userId, string chatRoomName, bool accept);
    }
}
