using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimpleChatApp.Hubs.Services
{
    public class FullLockUserHubContextManager : IUserHubContextManager
    {
        object _lock = new object();
        private readonly Dictionary<string, List<HubCallerContextWrapper>> _dict = new Dictionary<string, List<HubCallerContextWrapper>>();
        private readonly IHubContext<AppHub> _hubContext;

        public FullLockUserHubContextManager(IHubContext<AppHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void AddUserHubContext(string userId, HubCallerContextWrapper context)
        {
            lock (_dict)
            {
                if (_dict.TryAdd(userId, new List<HubCallerContextWrapper>() { context }))
                    return;

                var list = _dict[userId];
                list.Add(context);
            }
        }
        public void RemoveUserHubContext(string userId, HubCallerContextWrapper context)
        {
            lock (_dict)
            {
                if (!_dict.TryGetValue(userId, out var list))
                    return;
                list.Remove(context);
            }
        }

        public void RemoveAllUserHubContexts(string userId)
        {
            lock (_dict)
            {
                if (!_dict.Remove(userId, out var list))
                    return;
            }
        }

        public void AbortUserConnections(string userId)
        {
            lock (_dict)
            {
                if (!_dict.TryGetValue(userId, out var list))
                    return;

                foreach (var ctx in list)
                    ctx.AbortConnection();
            }
        }

        public List<string>? GetUserConnectionIds(string userId)
        {
            List<HubCallerContextWrapper>? list = null;
            lock (_dict)
            {
                _dict.TryGetValue(userId, out list);
            }
            return list?.Select(context => context.ConnectionId).ToList();
        }
    }
}
