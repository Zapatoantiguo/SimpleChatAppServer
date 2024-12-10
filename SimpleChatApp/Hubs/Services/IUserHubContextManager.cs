using Microsoft.AspNetCore.SignalR;

namespace SimpleChatApp.Hubs.Services
{
    /// <summary>
    /// Service for SignalR hub-related actions in link with particular user.
    /// Service provides an API for user hub contexts storage and some actions against hub contexts
    /// </summary>
    public interface IUserHubContextManager
    {
        public void AddUserHubContext(string userId, HubCallerContext context);
        public void RemoveUserHubContexts(string userId);
        public void Disconnect(string userId);
        public void RemoveFromGroups(string userId, List<string> groupNames);
        public void AddToGroups(string userId, List<string> groupNames);
        public List<HubCallerContext>? GetUserHubContexts(string userId);
        public List<string>? GetUserConnections(string userId);

    }
}
