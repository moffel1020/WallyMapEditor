using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public class SwfFileCache
{
    public ConcurrentDictionary<string, SwfFileData?> Cache { get; } = new();
    private readonly Queue<(string, SwfFileData)> _queue = new();
    private readonly HashSet<string> _queueSet = new();

    public void LoadSwf(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        Cache[path] = SwfFileData.CreateFrom(stream);
    }

    public async Task LoadSwfAsync(string path)
    {
        if (_queueSet.Contains(path)) return;
        _queueSet.Add(path);

        await Task.Run(() =>
        {
            Cache[path] = null;
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            SwfFileData swf = SwfFileData.CreateFrom(stream);
            lock (_queue) _queue.Enqueue((path, swf));
        });
    }

    public void UploadSwfs(int amount)
    {
        lock (_queue)
        {
            amount = Math.Clamp(amount, 0, _queue.Count);
            for (int i = 0; i < amount; i++)
            {
                (string path, SwfFileData swf) = _queue.Dequeue();
                _queueSet.Remove(path);
                Cache[path] = swf;
            }
        }
    }

    public void Clear()
    {
        Cache.Clear();
        _queueSet.Clear();
        lock (_queue) _queue.Clear();
    }
}