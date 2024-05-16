using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IMS = SixLabors.ImageSharp;

using SwfLib.Data;

using SwiffCheese.Exporting;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Wrappers;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

using TxtId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, ushort>;
using ImgData = System.ValueTuple<Raylib_cs.Image, SwfLib.Data.SwfRect>;

namespace WallyMapSpinzor2.Raylib;

public class SwfShapeCache
{
    public ConcurrentDictionary<TxtId, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(TxtId, ImgData)> _queue = new();
    private readonly HashSet<TxtId> _queueSet = [];

    public void LoadShape(SwfFileData swf, ushort shapeId)
    {
        (Raylib_cs.Image img, SwfRect rect) = LoadShapeInternal(swf, shapeId);
        Cache[(swf, shapeId)] = new(Rl.LoadTextureFromImage(img), rect);
        Rl.UnloadImage(img);
    }

    public void LoadShapeAsync(SwfFileData swf, ushort shapeId)
    {
        //HACK: too many shapes get loaded, causing the GPU to die. need to figure out a better way to do this.
        if (Random.Shared.Next(0, 100) != 6 || _queueSet.Count > 0 || _queueSet.Contains((swf, shapeId))) return;
        _queueSet.Add((swf, shapeId));

        Task.Run(() =>
        {
            (Raylib_cs.Image img, SwfRect rect) = LoadShapeInternal(swf, shapeId);
            lock (_queue) _queue.Enqueue(((swf, shapeId), (img, rect)));
        });
    }

    private static ImgData LoadShapeInternal(SwfFileData swf, ushort shapeId)
    {
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
                if (!Cache.ContainsKey(id))
                {
                    Texture2D texture = Rl.LoadTextureFromImage(img);
                    Cache[id] = new(texture, rect);
                }
                Rl.UnloadImage(img);
            }
        }
    }

    public void Clear()
    {
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