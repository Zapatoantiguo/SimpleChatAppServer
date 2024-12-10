using SimpleChatApp.ErrorHandling.ResultPattern;
using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    public interface INotificationDataService
    {
        public Task<Result<InviteNotification>> AddInviteNotificationAsync(InviteNotification notification);
        public Task<List<InviteNotification>> GetInviteNotifications(string userId);
    }
}
