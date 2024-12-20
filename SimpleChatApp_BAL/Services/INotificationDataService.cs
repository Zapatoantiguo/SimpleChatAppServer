using SimpleChatApp_BAL.ErrorHandling.ResultPattern;
using SimpleChatApp_DAL.Models.Notifications;

namespace SimpleChatApp_BAL.Services
{
    public interface INotificationDataService
    {
        public Task<Result<InviteNotification>> AddInviteNotificationAsync(InviteNotification notification);
        public Task<List<InviteNotification>> GetInviteNotifications(string userId);
    }
}
