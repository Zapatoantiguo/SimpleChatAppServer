using SimpleChatApp.Models.Notifications;

namespace SimpleChatApp.Data.Services
{
    public interface INotificationDataService
    {
        public Task<InviteNotification?> AddInviteNotificationAsync(InviteNotification notification);
        public Task<InviteNotification?> RemoveInviteNotificationAsync(InviteNotification notification);
        public Task<List<InviteNotification>> GetInviteNotifications(string userId);
    }
}
