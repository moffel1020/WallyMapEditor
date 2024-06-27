using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public abstract class UploadCache<K, I, V> where K : notnull
{
    public Dictionary<K, V> Cache { get; } = [];
    private readonly Queue<(K, I)> _queue = [];
    private readonly HashSet<K> _queueSet = [];

    protected abstract I LoadIntermediate(K k);
    protected abstract V IntermediateToValue(I i);
    protected abstract void UnloadIntermediate(I i);
    protected abstract void UnloadValue(V v);

    public void Load(K k)
    {
        if (Cache.ContainsKey(k))
            return;
        I i = LoadIntermediate(k);
        V v = IntermediateToValue(i);
        UnloadIntermediate(i);
        Cache[k] = v;
    }

    public void LoadInThread(K k)
    {
        if (Cache.ContainsKey(k) || _queueSet.Contains(k))
            return;
        _queueSet.Add(k);

        Task.Run(() =>
        {
            I i = LoadIntermediate(k);
            lock (_queue) _queue.Enqueue((k, i));
        });
    }

    public void Upload(int amount)
    {
        lock (_queue)
        {
            amount = Math.Clamp(amount, 0, _queue.Count);
            for (int j = 0; j < amount; j++)
            {
                (K k, I i) = _queue.Dequeue();
                _queueSet.Remove(k);
                if (!Cache.ContainsKey(k))
                {
                    V v = IntermediateToValue(i);
                    Cache[k] = v;
                }
                UnloadIntermediate(i);
            }
        }
    }

    public void Clear()
    {
        foreach ((_, V v) in Cache)
            UnloadValue(v);
        Cache.Clear();

        _queueSet.Clear();
        lock (_queue)
        {
            while (_queue.Count > 0)
            {
                (_, I i) = _queue.Dequeue();
                UnloadIntermediate(i);
            }
        }
    }
}