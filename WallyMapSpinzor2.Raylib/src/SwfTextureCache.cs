using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IMS = SixLabors.ImageSharp;

using SwfLib.Tags;

using SwiffCheese.Exporting;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Wrappers;

using Rl = Raylib_cs.Raylib;

using TxtId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, string>;
using TxtData = System.ValueTuple<WallyMapSpinzor2.Raylib.Texture2DWrapper, WallyMapSpinzor2.Transform>;
using ImgData = System.ValueTuple<Raylib_cs.Image, WallyMapSpinzor2.Transform>;

namespace WallyMapSpinzor2.Raylib;

public class SwfTextureCache
{
    public Dictionary<TxtId, TxtData> Cache { get; } = new();
    private readonly Queue<(TxtId, ImgData)> _queue = new();
    private readonly HashSet<TxtId> _queueSet = new();

    public void LoadTexture(SwfFileData swf, string name)
    {
        (Raylib_cs.Image img, Transform trans) = LoadImageInternal(swf, name);
        Cache[(swf, name)] = (new(Rl.LoadTextureFromImage(img)), trans);
    }

    public async Task LoadImageAsync(SwfFileData swf, string name)
    {
        if (_queueSet.Contains((swf, name))) return;
        _queueSet.Add((swf, name));
        await Task.Run(() =>
        {
            Cache[(swf, name)] = (Texture2DWrapper.Default, Transform.IDENTITY);
            (Raylib_cs.Image img, Transform trans) = LoadImageInternal(swf, name);
            lock (_queue)
            {
                _queue.Enqueue(((swf, name), (img, trans)));
            }
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
        IMS.Image<Rgba32> image = new(width, height, IMS.Color.Transparent.ToPixel<Rgba32>());
        ImageSharpShapeExporter exporter = new(image, new IMS.Size(-shape.ShapeBounds.XMin, -shape.ShapeBounds.YMin));
        compiledShape.Export(exporter);
        using MemoryStream ms = new();
        image.SaveAsPng(ms);
        Raylib_cs.Image img = Rl.LoadImageFromMemory(".png", ms.ToArray());
        Transform trans = Transform.CreateScale(0.05, 0.05) * Transform.CreateTranslate(x: shape.ShapeBounds.XMin, y: shape.ShapeBounds.YMin);
        return (img, trans);
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
                (Raylib_cs.Image img, Transform trans) = dat;
                Cache[id] = (new(Rl.LoadTextureFromImage(img)), trans);
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