using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SimpleChatApp.Hubs.Services
{
    public class UserHubContextManager : IUserHubContextManager
    {
        object _lock = new object();
        private readonly Dictionary<string, List<HubCallerContext>> _dict = new Dictionary<string, List<HubCallerContext>>();
        private readonly IGroupManager _groupManager;

        public UserHubContextManager(IGroupManager groupManager)
        {
            _groupManager = groupManager;
        }

        public void AddUserHubContext(string userId, HubCallerContext context)
        {
            lock (_dict)
            {
                if (_dict.TryAdd(userId, new List<HubCallerContext>() { context }))
                    return;

                var list = _dict[userId];
                list.Add(context);
            }
        }

        void IUserHubContextManager.RemoveUserHubContexts(string userId)
        {
            lock (_dict)
            {
                if (!_dict.Remove(userId, out var list))
                    return;
            }
        }

        void IUserHubContextManager.Disconnect(string userId)
        {
            lock (_dict)
            {
                if (!_dict.TryGetValue(userId, out var list))
                    return;

                foreach (var ctx in list)
                    ctx.Abort();
            }
        }

        void IUserHubContextManager.RemoveFromGroups(string userId, List<string> groupNames)
        {
            lock (_dict)
            {
                if (!_dict.TryGetValue(userId, out var list))
                    return;

                foreach (var ctx in list)
                    foreach (var group in groupNames)
                        _groupManager.RemoveFromGroupAsync(ctx.ConnectionId, group);
            }
        }

        void IUserHubContextManager.AddToGroups(string userId, List<string> groupNames)
        {
            lock (_dict)
            {
                if (!_dict.TryGetValue(userId, out var list))
                    return;

                foreach (var ctx in list)
                    foreach (var group in groupNames)
                        _groupManager.AddToGroupAsync(ctx.ConnectionId, group);
            }
        }
    }
}
