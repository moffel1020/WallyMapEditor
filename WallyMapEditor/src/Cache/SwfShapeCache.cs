using System;
using System.Xml;

using SwiffCheese.Shapes;
using SwiffCheese.Wrappers;
using SwiffCheese.Exporting.Svg;

using Raylib_cs;

using Svg.Skia;
using SkiaSharp;

namespace WallyMapEditor;

public sealed class SwfShapeCache : UploadCache<SwfShapeCache.TextureInfo, SwfShapeCache.ShapeData, Texture2DWrapper>
{
    private readonly static SKSamplingOptions SAMPLING_OPTIONS = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    private const int SWF_UNIT_DIVISOR = 20;
    private const double ANIM_SCALE_MULTIPLIER = 1.2;

    public readonly record struct TextureInfo(SwfFileData Swf, ushort ShapeId, double AnimScale);
    public readonly record struct ShapeData(RlImage Image, WmsTransform Transform);

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

        WmsTransform transform = WmsTransform.CreateScale(animScale, animScale) * WmsTransform.CreateTranslate(-offsetX, -offsetY);

        SvgSize size = new(imageW, imageH);
        SvgMatrix matrix = new(transform.ScaleX, transform.SkewY, transform.SkewX, transform.ScaleY, transform.TranslateX, transform.TranslateY);
        SvgShapeExporter exporter = new(size, matrix);
        compiledShape.Export(exporter);

        using XmlReader reader = exporter.Document.CreateReader();
        using SKSvg svg = SKSvg.CreateFromXmlReader(reader);
        reader.Dispose();
        using SKBitmap bitmap1 = svg.Picture!.ToBitmap(SKColors.Transparent, 3, 3, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb())!;
        svg.Dispose();
        using SKBitmap bitmap2 = bitmap1.Resize(new SKSizeI(imageW, imageH), SAMPLING_OPTIONS);
        bitmap1.Dispose();
        RlImage img1 = WmeUtils.SKBitmapAsRlImage(bitmap2);
        RlImage img2 = RaylibEx.ImageCopyWithMipmaps(img1);
        bitmap2.Dispose(); // also unloads img1

        WmsTransform inv = WmsTransform.CreateInverse(transform);
        return new ShapeData(img2, inv);
    }

    protected override Texture2DWrapper IntermediateToValue(ShapeData shapeData)
    {
        (RlImage img, WmsTransform trans) = shapeData;
        Texture2D texture = Rl.LoadTextureFromImage(img);
        return new(texture, trans);
    }

    protected override void InitValue(Texture2DWrapper v)
    {
        Texture2D texture = v.Texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        Rl.GenTextureMipmaps(ref texture);
    }

    protected override void UnloadIntermediate(ShapeData shapeData)
    {
        Rl.UnloadImage(shapeData.Image);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }

    public void Load(SwfFileData swf, ushort shapeId, double animScale) => Load(new(swf, shapeId, animScale));
    public void LoadInThread(SwfFileData swf, ushort shapeId, double animScale) => LoadInThread(new(swf, shapeId, animScale));
}