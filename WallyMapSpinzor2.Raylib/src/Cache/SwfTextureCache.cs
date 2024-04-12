using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IMS = SixLabors.ImageSharp;

using SwfLib.Tags;
using SwfLib.Data;

using SwiffCheese.Exporting;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Wrappers;

using Rl = Raylib_cs.Raylib;

using TxtId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, string>;
using ImgData = System.ValueTuple<Raylib_cs.Image, SwfLib.Data.SwfRect>;

namespace WallyMapSpinzor2.Raylib;

public class SwfTextureCache
{
    public ConcurrentDictionary<TxtId, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(TxtId, ImgData)> _queue = new();
    private readonly HashSet<TxtId> _queueSet = [];

    public void LoadTexture(SwfFileData swf, string name)
    {
        (Raylib_cs.Image img, SwfRect rect) = LoadImageInternal(swf, name);
        Cache[(swf, name)] = new(Rl.LoadTextureFromImage(img), rect);
        Rl.UnloadImage(img);
    }

    public async Task LoadImageAsync(SwfFileData swf, string name)
    {
        if (_queueSet.Contains((swf, name))) return;
        _queueSet.Add((swf, name));

        await Task.Run(() =>
        {
            Cache[(swf, name)] = Texture2DWrapper.Default;
            (Raylib_cs.Image img, SwfRect rect) = LoadImageInternal(swf, name);
            lock (_queue) _queue.Enqueue(((swf, name), (img, rect)));
        });
    }

    private static ImgData LoadImageInternal(SwfFileData swf, string name)
    {
        ushort spriteId = swf.SymbolClass[name];
        DefineSpriteTag sprite = swf.SpriteTags[spriteId];
        //we currently only load the first shape
        //NOTE: this will need to be changed in the future. fine for now.
        ushort shapeId = sprite.GetShapeIds().FirstOrDefault();
        DefineShapeXTag shape = swf.ShapeTags[shapeId];
        SwfShape compiledShape = new(shape);
        int width = shape.ShapeBounds.Width();
        int height = shape.ShapeBounds.Height();
        using Image<Rgba32> image = new(width, height, IMS.Color.Transparent.ToPixel<Rgba32>());
        ImageSharpShapeExporter exporter = new(image, new Size(-shape.ShapeBounds.XMin, -shape.ShapeBounds.YMin));
        compiledShape.Export(exporter);
        Raylib_cs.Image img = Utils.ImageSharpImageToRl(image);
        return (img, shape.ShapeBounds);
    }

    public void UploadImages(int amount)
    {
        lock (_queue)
        {
            amount = Math.Clamp(amount, 0, _queue.Count);
            for (int i = 0; i < amount; i++)
            {
                (TxtId id, ImgData dat) = _queue.Dequeue();
                _queueSet.Remove(id);
                (Raylib_cs.Image img, SwfRect rect) = dat;
                Cache[id] = new(Rl.LoadTextureFromImage(img), rect);
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
                (_, (Raylib_cs.Image img, _)) = _queue.Dequeue();
                Rl.UnloadImage(img);
            }
        }
    }
}