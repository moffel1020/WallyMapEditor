using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IMS = SixLabors.ImageSharp;

using SwiffCheese.Exporting;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Wrappers;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

using TxtId = System.ValueTuple<WallyMapSpinzor2.Raylib.SwfFileData, ushort, double>;
using ImgData = System.ValueTuple<Raylib_cs.Image, int, int, double>;

namespace WallyMapSpinzor2.Raylib;

public class SwfShapeCache : UploadCache<TxtId, ImgData, Texture2DWrapper>
{
    private const int SWF_UNIT_DIVISOR = 20;
    private const double ANIM_SCALE_MULTIPLIER = 1.2;

    protected override ImgData LoadIntermediate(TxtId data)
    {
        (SwfFileData swf, ushort shapeId, double animScale) = data;
        animScale *= ANIM_SCALE_MULTIPLIER;
        DefineShapeXTag shape = swf.ShapeTags[shapeId];
        SwfShape compiledShape = new(shape);
        // logic follows game
        double x = shape.ShapeBounds.XMin * 1.0 / SWF_UNIT_DIVISOR;
        double y = shape.ShapeBounds.YMin * 1.0 / SWF_UNIT_DIVISOR;
        double w = shape.ShapeBounds.Width() * animScale / SWF_UNIT_DIVISOR;
        double h = shape.ShapeBounds.Height() * animScale / SWF_UNIT_DIVISOR;
        int offsetX = (int)Math.Floor(x);
        int offsetY = (int)Math.Floor(y);
        int imageW = (int)Math.Floor(w + (x - offsetX) + animScale) + 2;
        int imageH = (int)Math.Floor(h + (y - offsetY) + animScale) + 2;
        using Image<Rgba32> image = new(imageW, imageH, IMS.Color.Transparent.ToPixel<Rgba32>());
        ImageSharpShapeExporter exporter = new(image, new Size(SWF_UNIT_DIVISOR * -offsetX, SWF_UNIT_DIVISOR * -offsetY), SWF_UNIT_DIVISOR);
        compiledShape.Export(exporter);
        Raylib_cs.Image img = Utils.ImageSharpImageToRl(image);
        // brawlhalla uses the un-multiplied AnimScale for the actual scaling
        return (img, offsetX, offsetY, animScale / ANIM_SCALE_MULTIPLIER);
    }

    protected override Texture2DWrapper IntermediateToValue(ImgData intermediate)
    {
        (Raylib_cs.Image img, int offsetX, int offsetY, double animScale) = intermediate;
        Texture2D texture = Rl.LoadTextureFromImage(img);
        return new(texture, offsetX, offsetY, animScale);
    }

    protected override void UnloadIntermediate(ImgData intermediate)
    {
        (Raylib_cs.Image img, _, _, _) = intermediate;
        Rl.UnloadImage(img);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }

    public void LoadAsync(SwfFileData swf, ushort shapeId, double animScale)
    {
        LoadAsync((swf, shapeId, animScale));
    }
}