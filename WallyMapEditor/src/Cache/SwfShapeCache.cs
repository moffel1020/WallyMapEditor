using System;
using System.Numerics;
using System.Xml;
using System.IO;

using SwiffCheese.Shapes;
using SwiffCheese.Wrappers;
using SwiffCheese.Exporting.Svg;
using SwiffCheese.Math;

using Raylib_cs;

using Svg.Skia;
using SkiaSharp;

namespace WallyMapEditor;

public class SwfShapeCache : UploadCache<SwfShapeCache.TextureInfo, SwfShapeCache.ShapeData, Texture2DWrapper>
{
    private const int RASTER_SCALE = 1;
    private const int SWF_UNIT_DIVISOR = 20;
    private const double ANIM_SCALE_MULTIPLIER = 1.2;

    public readonly record struct TextureInfo(SwfFileData Swf, ushort ShapeId, double AnimScale);
    public readonly record struct ShapeData(RlImage Img, int OffsetX, int OffsetY);

    protected override ShapeData LoadIntermediate(TextureInfo textureInfo)
    {
        (SwfFileData swf, ushort shapeId, double animScale) = textureInfo;
        animScale *= ANIM_SCALE_MULTIPLIER;
        DefineShapeXTag shape = swf.ShapeTags[shapeId];
        SwfShape compiledShape = new(shape);
        // logic follows game
        int shapeX = shape.ShapeBounds.XMin;
        int shapeY = shape.ShapeBounds.YMin;
        int shapeW = shape.ShapeBounds.XMax - shape.ShapeBounds.XMin;
        int shapeH = shape.ShapeBounds.YMax - shape.ShapeBounds.YMin;

        double x = shapeX * 1.0 / SWF_UNIT_DIVISOR;
        double y = shapeY * 1.0 / SWF_UNIT_DIVISOR;
        double w = shapeW * animScale / SWF_UNIT_DIVISOR;
        double h = shapeH * animScale / SWF_UNIT_DIVISOR;
        int offsetX = (int)Math.Floor(x);
        int offsetY = (int)Math.Floor(y);
        int imageW = (int)Math.Floor(w + (x - offsetX) + animScale) + 2;
        int imageH = (int)Math.Floor(h + (y - offsetY) + animScale) + 2;

        Vector2I position = new(offsetX, offsetY);
        Vector2I size = new(imageW, imageH);
        SvgShapeExporter exporter = new(position * SWF_UNIT_DIVISOR, size * SWF_UNIT_DIVISOR, SWF_UNIT_DIVISOR);
        compiledShape.Export(exporter);
        exporter.Document.Root?.SetAttributeValue("shape-rendering", "crispEdges");

        using XmlReader reader = exporter.Document.CreateReader();
        using SKSvg svg = SKSvg.CreateFromXmlReader(reader);
        reader.Dispose();

        using SKBitmap bitmap = svg.Picture!.ToBitmap(SKColors.Transparent, 1, 1, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb())!;
        svg.Dispose();

        RlImage img = WmeUtils.SKBitmapToRlImage(bitmap);
        bitmap.Dispose();

        // no need for alpha premult since we specify it in the ToBitmap

        return new ShapeData(img, offsetX, offsetY);
    }

    protected override Texture2DWrapper IntermediateToValue(ShapeData shapeData)
    {
        (RlImage img, int offsetX, int offsetY) = shapeData;
        Texture2D texture = Rl.LoadTextureFromImage(img);
        Rl.SetTextureFilter(texture, TextureFilter.Bilinear);
        return new(texture, offsetX, offsetY);
    }

    protected override void UnloadIntermediate(ShapeData shapeData)
    {
        Rl.UnloadImage(shapeData.Img);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }

    public void Load(SwfFileData swf, ushort shapeId, double animScale) => Load(new(swf, shapeId, animScale));
    public void LoadInThread(SwfFileData swf, ushort shapeId, double animScale) => LoadInThread(new(swf, shapeId, animScale));
}