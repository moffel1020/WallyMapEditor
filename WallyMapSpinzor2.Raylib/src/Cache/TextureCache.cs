using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class TextureCache
{
    public ConcurrentDictionary<string, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(string, Image)> _queue = new();
    private readonly HashSet<string> _queueSet = [];

    public void LoadTexture(string path)
    {
        Image img = Utils.LoadRlImage(path);
        Texture2D texture = Rl.LoadTextureFromImage(img);
        Cache[path] = new(texture);
        Rl.UnloadImage(img);
    }

    public async Task LoadImageAsync(string path)
    {
        if (_queueSet.Contains(path)) return;
        _queueSet.Add(path);

        await Task.Run(() =>
        {
            Image img = Utils.LoadRlImage(path);
            lock (_queue) _queue.Enqueue((path, img));
        });
    }

    public void UploadImages(int amount)
    {
        lock (_queue)
        {
            amount = Math.Clamp(amount, 0, _queue.Count);
            for (int i = 0; i < amount; i++)
            {
                (string path, Image img) = _queue.Dequeue();
                _queueSet.Remove(path);
                if (Cache.TryGetValue(path, out Texture2DWrapper? oldTexture))
                    oldTexture.Dispose();
                Texture2D texture = Rl.LoadTextureFromImage(img);
                Cache[path] = new(texture);
                Rl.UnloadImage(img);
            }
        }
    }

    public void Clear()
    {
        // manually dispose textures.
        // raylib doesn't like it if you do this through GC.
        foreach ((_, Texture2DWrapper txt) in Cache)
            txt.Dispose();
        Cache.Clear();

        _queueSet.Clear();
        lock (_queue)
        {
            while (_queue.Count > 0)
            {
                (_, Image img) = _queue.Dequeue();
                Rl.UnloadImage(img);
            }
        }
    }
}