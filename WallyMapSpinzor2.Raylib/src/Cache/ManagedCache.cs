using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WallyMapEditor;

public abstract class ManagedCache<K, V> where K : notnull
{
    public ConcurrentDictionary<K, V> Cache { get; } = [];
    private readonly HashSet<K> _loading = [];

    protected abstract V LoadInternal(K k);

    public void Load(K k)
    {
        if (Cache.ContainsKey(k))
            return;
        V v = LoadInternal(k);
        Cache[k] = v;
    }

    public void LoadInThread(K k)
    {
        if (Cache.ContainsKey(k))
            return;
        lock (_loading)
        {
            if (_loading.Contains(k))
                return;
            _loading.Add(k);
        }

        Task.Run(() =>
        {
            Load(k);
            lock (_loading)
            {
                _loading.Remove(k);
            }
        });
    }

    public void Clear()
    {
        Cache.Clear();
    }
}