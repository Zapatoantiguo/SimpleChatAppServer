using SimpleChatApp_BAL.Tools;

namespace SimpleChatApp.Hubs.Services
{
    public class EntryLockUserHubContextManager : IUserHubContextManager
    {
        private readonly ConcurrentMultiValueDict<string, HubCallerContextWrapper> _storage;
        public EntryLockUserHubContextManager()
        {
            _storage = new ConcurrentMultiValueDict<string, HubCallerContextWrapper>();
        }
        public void AddUserHubContext(string userId, HubCallerContextWrapper context)
        {
            var isAdded =_storage.Add(userId, context);

            if (!isAdded) throw new Exception($"Trying to add existing context. userId: {userId}, " +
                $"connectionId: {context.ConnectionId}");
        }
        public void RemoveUserHubContext(string userId, HubCallerContextWrapper context)
        {
            var isRemoved = _storage.Remove(userId, context);

            if (!isRemoved) throw new Exception($"Trying to remove absent context. userId: {userId}, " +
                $"connectionId: {context.ConnectionId}");
        }
        public void RemoveAllUserHubContexts(string userId)
        {
            throw new NotImplementedException();
        }
        public List<string>? GetUserConnectionIds(string userId)
        {
            _storage.TryGetItems(userId, out var items);
            if (items == null) return null;
            return items.Select(ctx => ctx.ConnectionId).ToList();
        }
        public void AbortUserConnections(string userId)
        {
            _storage.TryGetItems(userId, out var items);
            if (items == null) return;

            foreach (var ctx in items)
                ctx.AbortConnection();
        }
    }
}
