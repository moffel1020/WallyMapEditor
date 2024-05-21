using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IMS = SixLabors.ImageSharp;

using SwiffCheese.Exporting;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Wrappers;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

using TxtId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, ushort>;
using ImgData = System.ValueTuple<Raylib_cs.Image, int, int, double>;

namespace WallyMapSpinzor2.Raylib;

public class SwfShapeCache
{
    public ConcurrentDictionary<TxtId, Texture2DWrapper> Cache { get; } = new();
    private readonly Queue<(TxtId, ImgData)> _queue = new();
    private readonly HashSet<TxtId> _queueSet = [];

    public void LoadShape(SwfFileData swf, ushort shapeId, double quality)
    {
        (Raylib_cs.Image img, int offsetX, int offsetY, _) = LoadShapeInternal(swf, shapeId, quality);
        Texture2D texture = Rl.LoadTextureFromImage(img);
        Cache[(swf, shapeId)] = new(texture, offsetX, offsetY, quality);
        Rl.UnloadImage(img);
    }

    public void LoadShapeAsync(SwfFileData swf, ushort shapeId, double quality)
    {
        if (_queueSet.Contains((swf, shapeId))) return;
        _queueSet.Add((swf, shapeId));

        Task.Run(() =>
        {
            (Raylib_cs.Image img, int offsetX, int offsetY, _) = LoadShapeInternal(swf, shapeId, quality);
            lock (_queue) _queue.Enqueue(((swf, shapeId), (img, offsetX, offsetY, quality)));
        });
    }

    private const int SWF_UNIT_DIVISOR = 20;
    // this can actually change per GfxType (AnimScale property multiplies this). later.
    private const double QUALITY_MULTIPLIER = 1.2;

    private static ImgData LoadShapeInternal(SwfFileData swf, ushort shapeId, double quality)
    {
        quality *= QUALITY_MULTIPLIER;
        DefineShapeXTag shape = swf.ShapeTags[shapeId];
        SwfShape compiledShape = new(shape);
        // logic follows game
        double x = shape.ShapeBounds.XMin * 1.0 / SWF_UNIT_DIVISOR;
        double y = shape.ShapeBounds.YMin * 1.0 / SWF_UNIT_DIVISOR;
        double w = shape.ShapeBounds.Width() * quality / SWF_UNIT_DIVISOR;
        double h = shape.ShapeBounds.Height() * quality / SWF_UNIT_DIVISOR;
        int offsetX = (int)Math.Floor(x);
        int offsetY = (int)Math.Floor(y);
        int imageW = (int)Math.Floor(w + (x - offsetX) + quality) + 2;
        int imageH = (int)Math.Floor(h + (y - offsetY) + quality) + 2;
        using Image<Rgba32> image = new(imageW, imageH, IMS.Color.Transparent.ToPixel<Rgba32>());
        ImageSharpShapeExporter exporter = new(image, new Size(SWF_UNIT_DIVISOR * -offsetX, SWF_UNIT_DIVISOR * -offsetY), SWF_UNIT_DIVISOR);
        compiledShape.Export(exporter);
        Raylib_cs.Image img = Utils.ImageSharpImageToRl(image);
        return (img, offsetX, offsetY, quality);
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
                (Raylib_cs.Image img, int offsetX, int offsetY, double animScale) = dat;
                if (!Cache.ContainsKey(id))
                {
                    Texture2D texture = Rl.LoadTextureFromImage(img);
                    Cache[id] = new(texture, offsetX, offsetY, animScale);
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
                (_, (Raylib_cs.Image img, _, _, _)) = _queue.Dequeue();
                Rl.UnloadImage(img);
            }
        }
    }
}