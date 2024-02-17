using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class TextureCache
{
    public Dictionary<string, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(string, Image)> _queue = new();
    private readonly HashSet<string> _queueSet = new();

    public void LoadTexture(string path)
    {
        Image img = Utils.LoadRlImage(path);
        Texture2D texture = Rl.LoadTextureFromImage(img);
        Cache[path] = new(texture);
    }

    public async Task LoadImageAsync(string path)
    {
        if (_queueSet.Contains(path)) return;
        _queueSet.Add(path);
        await Task.Run(() =>
        {
            Cache[path] = Texture2DWrapper.Default;
            Image img = Utils.LoadRlImage(path);
            lock (_queue)
            {
                _queue.Enqueue((path, img));
            }
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
                Cache[path] = new(Rl.LoadTextureFromImage(img));
            }
        }
    }

    public void Clear()
    {
        Cache.Clear();
        _queueSet.Clear();
        lock (_queue)
        {
            _queue.Clear();
        }
    }
}