using System.Collections.Concurrent;

namespace SimpleChatApp_BAL.Tools
{
    public class ConcurrentMultiValueDict<TKey, TValue>
    {
        private class EntryValue : HashSet<TValue>
        {
            public bool MarkedToDelete { get; set; }
        }
        private readonly ConcurrentDictionary<TKey, EntryValue> _dict;
        public ConcurrentMultiValueDict()
        {
            _dict = new ConcurrentDictionary<TKey, EntryValue>();
        }
        public int EntriesCount => _dict.Count;
        public bool Add(TKey key, TValue item)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                var set = _dict.GetOrAdd(key, new EntryValue());
                lock (set)
                {
                    if (!set.MarkedToDelete) return set.Add(item);
                }
                spinWait.SpinOnce();
            }
        }
        public bool Remove(TKey key, TValue value)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                if (!_dict.TryGetValue(key, out var set))
                    return false;
                lock (set)
                {
                    if (set.MarkedToDelete)
                    {
                        spinWait.SpinOnce();
                        continue;
                    }
                    else
                    {
                        bool itemRemoved = set.Remove(value);
                        if (!itemRemoved) return false;
                        if (set.Count != 0) return false;
                        set.MarkedToDelete = true;
                    }
                }
                bool entryRemoved = _dict.TryRemove(key, out var currentSet);
                return true;
            }
        }

        public bool TryGetItems(TKey key, out TValue[]? items)
        {
            if (!_dict.TryGetValue(key, out var set))
            {
                items = null;
                return false;
            }
            bool markedToDelete;
            lock (set)
            {
                markedToDelete = set.MarkedToDelete;
                items = set.ToArray();
            }
            if (markedToDelete || items.Count() == 0)   // count check for case: entry added already but item is not yet
            {
                items = null;
                return false;
            }
            return true;
        }
    }
}
