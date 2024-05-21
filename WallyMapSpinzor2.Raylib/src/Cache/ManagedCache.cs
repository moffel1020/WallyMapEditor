using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public abstract class ManagedCache<K, V> where K : notnull
{
    public ConcurrentDictionary<K, V> Cache { get; } = new();
    private HashSet<K> _loading = [];

    protected abstract V LoadInternal(K k);

    public void Load(K k)
    {
        V v = LoadInternal(k);
        Cache[k] = v;
    }

    public void LoadAsync(K k)
    {
        lock (_loading)
        {
            if (_loading.Contains(k) || Cache.ContainsKey(k)) return;
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