using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class TextureCache
{
    public Dictionary<string, Texture2DWrapper> Cache { get; } = new();
    private Queue<(string path, Image)> Images { get; set; } = new();

    public void LoadTexture(string path)
    {
        Texture2D texture = Utils.LoadRlTexture(path);
        Cache[path] = new(texture);
    }

    public async Task LoadImageAsync(string path)
    {
        await Task.Run(() =>
        {
            Cache[path] = Texture2DWrapper.Default;
            Image img = Utils.LoadRlImage(path);
            lock (Images)
            {
                Images.Enqueue((path, img));
            }
        });
    }

    public void UploadImages(int amount)
    {
        lock (Images)
        {
            amount = Math.Clamp(amount, 0, Images.Count);
            for (int i = 0; i < amount; i++)
            {
                (string path, Image img) = Images.Dequeue();
                Cache[path] = new(Rl.LoadTextureFromImage(img));
            }
        }
    }

    public void Clear()
    {
        Cache.Clear(); 
        lock (Images) Images.Clear();
    }
}