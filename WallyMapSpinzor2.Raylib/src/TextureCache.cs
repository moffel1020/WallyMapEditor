using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class TextureCache
{
    public Dictionary<string, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(string, Image)> _images = new();
    private readonly HashSet<string> _queueSet = new();

    public void LoadTexture(string path)
    {
        Texture2D texture = Utils.LoadRlTexture(path);
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
            lock (_images)
            {
                _images.Enqueue((path, img));
            }
        });
    }

    public void UploadImages(int amount)
    {
        lock (_images)
        {
            amount = Math.Clamp(amount, 0, _images.Count);
            for (int i = 0; i < amount; i++)
            {
                (string path, Image img) = _images.Dequeue();
                _queueSet.Remove(path);
                Cache[path] = new(Rl.LoadTextureFromImage(img));
            }
        }
    }

    public void Clear()
    {
        Cache.Clear();
        _queueSet.Clear();
        lock (_images)
        {
            _images.Clear();
        }
    }
}