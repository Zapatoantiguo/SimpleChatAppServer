using Microsoft.AspNetCore.SignalR;

namespace SimpleChatApp.Hubs.Services
{
    /// <summary>
    /// Service for SignalR hub-related actions in link with particular user.
    /// Service provides an API for user hub contexts storage and some actions against hub contexts
    /// </summary>
    public interface IUserHubContextManager
    {
        public void AddUserHubContext(string userId, HubCallerContextWrapper context);
        public void RemoveUserHubContext(string userId, HubCallerContextWrapper context);
        public void RemoveAllUserHubContexts(string userId);
        public void AbortUserConnections(string userId);
        public List<string>? GetUserConnectionIds(string userId);

    }

    public class HubCallerContextWrapper
    {
        public string ConnectionId { get; }
        private readonly HubCallerContext _hubCallerContext;
        public HubCallerContextWrapper(HubCallerContext hubCallerContext)
        {
            _hubCallerContext = hubCallerContext;
            ConnectionId = hubCallerContext.ConnectionId;
        }
        public void AbortConnection()
        {
            _hubCallerContext.Abort();
        }
        public override int GetHashCode()
        {
            return ConnectionId.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is not HubCallerContextWrapper other)
                return false;

            return this.ConnectionId == other.ConnectionId;
        }
    }
}
