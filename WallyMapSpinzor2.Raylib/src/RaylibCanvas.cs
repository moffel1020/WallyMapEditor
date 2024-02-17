using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;

using Rl = Raylib_cs.Raylib;
using Raylib_cs;

using SwiffCheese.Wrappers;
using SwiffCheese.Shapes;
using SwiffCheese.Utils;
using SwiffCheese.Exporting;

using IS = SixLabors.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using SwfLib.Tags;

namespace WallyMapSpinzor2.Raylib;

public class RaylibCanvas : ICanvas<Texture2DWrapper>
{
    public string BrawlPath { get; set; }
    public BucketPriorityQueue<Action> DrawingQueue { get; set; } = new(Enum.GetValues<DrawPriorityEnum>().Length);
    public TextureCache TextureCache { get; set; } = new();
    public SwfFileCache SwfFileCache { get; set; } = new();
    public Dictionary<(string, string), Texture2DWrapper> SwfTextureCache { get; } = new();
    public Dictionary<Texture2DWrapper, Transform> TextureTransform { get; } = new();
    public Matrix4x4 CameraMatrix { get; set; } = Matrix4x4.Identity;

    public RaylibCanvas(string brawlPath)
    {
        BrawlPath = brawlPath;
    }

    public void ClearTextureCache()
    {
        TextureCache.Clear();
    }

    public void DrawCircle(double x, double y, double radius, Color color, Transform trans, DrawPriorityEnum priority)
    {
        // FIXME: doesn't account for transformations affecting radius (could be turned into an ellipse)
        (x, y) = trans * (x, y);

        DrawingQueue.Push(() =>
        {
            Rl.DrawCircle((int)x, (int)y, (float)radius, Utils.ToRlColor(color));
        }, (int)priority);
    }

    public void DrawLine(double x1, double y1, double x2, double y2, Color color, Transform trans, DrawPriorityEnum priority)
    {
        (x1, y1) = trans * (x1, y1);
        (x2, y2) = trans * (x2, y2);

        DrawingQueue.Push(() =>
        {
            Rl.DrawLine((int)x1, (int)y1, (int)x2, (int)y2, Utils.ToRlColor(color));
        }, (int)priority);
    }

    public const double MULTI_COLOR_LINE_OFFSET = 5;
    public void DrawLineMultiColor(double x1, double y1, double x2, double y2, Color[] colors, Transform trans, DrawPriorityEnum priority)
    {
        (x1, y1) = trans * (x1, y1);
        (x2, y2) = trans * (x2, y2);
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        double center = (colors.Length - 1) / 2.0;
        (double offX, double offY) = (y1 - y2, x2 - x1);
        (offX, offY) = BrawlhallaMath.Normalize(offX, offY);
        for (int i = 0; i < colors.Length; ++i)
        {
            double mult = MULTI_COLOR_LINE_OFFSET * (i - center);
            DrawLine(x1 + offX * mult, y1 + offY * mult, x2 + offX * mult, y2 + offY * mult, colors[i], Transform.IDENTITY, priority);
        }
        // version that should be camera-independent. doesn't work properly.
        // line offset ends up being really big when far away
        /*
        Debug.Assert(Matrix4x4.Invert(CameraMatrix, out Matrix4x4 invertedMat));
        Transform cam = Utils.MatrixToTransform(CameraMatrix);
        Transform inv = Utils.MatrixToTransform(invertedMat);

        (x1, y1) = cam * trans * (x1, y1);
        (x2, y2) = cam * trans * (x2, y2);
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        double center = (colors.Length - 1) / 2.0;
        (double offX, double offY) = (y1 - y2, x2 - x1);
        (offX, offY) = BrawlhallaMath.Normalize(offX, offY);
        for (int i = 0; i < colors.Length; ++i)
        {
            double mult = MULTI_COLOR_LINE_OFFSET * (i - center);
            DrawLine(x1 + offX * mult, y1 + offY * mult, x2 + offX * mult, y2 + offY * mult, colors[i], inv, priority);
        }
        */
    }

    public void DrawRect(double x, double y, double w, double h, bool filled, Color color, Transform trans, DrawPriorityEnum priority)
    {
        DrawingQueue.Push(() =>
        {
            if (filled)
            {
                DrawRectWithTransform(x, y, w, h, trans, color);
            }
            else
            {
                DrawLine(x, y, x + w, y, color, trans, priority);
                DrawLine(x + w, y, x + w, y + h, color, trans, priority);
                DrawLine(x + w, y + h, x, y + h, color, trans, priority);
                DrawLine(x, y + h, x, y, color, trans, priority);
            }
        }, (int)priority);
    }

    public void DrawString(double x, double y, string text, double fontSize, Color color, Transform trans, DrawPriorityEnum priority)
    {

    }

    public void DrawTexture(double x, double y, Texture2DWrapper texture, Transform trans, DrawPriorityEnum priority)
    {
        Transform textureTrans = TextureTransform.GetValueOrDefault(texture, Transform.IDENTITY);
        trans *= textureTrans;

        Texture2D rlTexture = (Texture2D)texture.Texture;
        DrawingQueue.Push(() =>
        {
            DrawTextureWithTransform(rlTexture, x, y, rlTexture.Width, rlTexture.Height, trans, Color.FromHex(0xFFFFFFFF));
        }, (int)priority);
    }

    public void DrawTextureRect(double x, double y, double w, double h, Texture2DWrapper texture, Transform trans, DrawPriorityEnum priority)
    {
        Transform textureTrans = TextureTransform.GetValueOrDefault(texture, Transform.IDENTITY);
        trans *= textureTrans;

        Texture2D rlTexture = (Texture2D)texture.Texture;
        DrawingQueue.Push(() =>
        {
            DrawTextureWithTransform(rlTexture, x, y, w, h, trans, Color.FromHex(0xFFFFFFFF));
        }, (int)priority);
    }

    private static void DrawTextureWithTransform(Texture2D texture, double x, double y, double w, double h, Transform trans, Color color)
    {
        Rlgl.SetTexture(texture.Id);
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] texCoords = new (double, double)[] { (0, 0), (0, 1), (1, 1), (1, 0), (0, 0) };
        (double, double)[] points = new (double, double)[] { trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin) };
        // we need to ensure that the points are in a counter clockwise order.
        // but no matter what i do there's always some sprite that still disappears.
        // so fuck it, just draw it twice.
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.TexCoord2f((float)texCoords[i].Item1, (float)texCoords[i].Item2);
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.TexCoord2f((float)texCoords[i + 1].Item1, (float)texCoords[i + 1].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Array.Reverse(texCoords);
        Array.Reverse(points);
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.TexCoord2f((float)texCoords[i].Item1, (float)texCoords[i].Item2);
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.TexCoord2f((float)texCoords[i + 1].Item1, (float)texCoords[i + 1].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
        Rlgl.SetTexture(0);
    }

    private static void DrawRectWithTransform(double x, double y, double w, double h, Transform trans, Color color)
    {
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] points = new (double, double)[] { trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin) };
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Array.Reverse(points);
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
    }

    public Texture2DWrapper LoadTextureFromPath(string path)
    {
        string finalPath = Path.Combine(BrawlPath, "mapArt", path);
        TextureCache.Cache.TryGetValue(finalPath, out Texture2DWrapper? texture);
        if (texture is not null) return texture;

        _ = TextureCache.LoadImageAsync(finalPath);
        return Texture2DWrapper.Default; // placeholder white texture until the image is read from disk
    }

    public Texture2DWrapper LoadTextureFromSWF(string filePath, string name)
    {
        string finalPath = Path.Combine(BrawlPath, filePath);
        SwfTextureCache.TryGetValue((finalPath, name), out Texture2DWrapper? texture);
        if (texture is not null) return texture;
        SwfFileCache.Cache.TryGetValue(finalPath, out SwfFileData? cache);
        if (cache is not null)
        {
            (Texture2DWrapper swfTexture, Transform trans) = LoadTextureFromSwf(cache, name);
            SwfTextureCache.Add((finalPath, name), swfTexture);
            TextureTransform.Add(swfTexture, trans);
            return swfTexture;
        }

        _ = SwfFileCache.LoadSwfAsync(finalPath);
        return Texture2DWrapper.Default;
    }

    private static (Texture2DWrapper, Transform) LoadTextureFromSwf(SwfFileData swf, string name)
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
        Image<Rgba32> image = new(width, height, IS.Color.Transparent.ToPixel<Rgba32>());
        ImageSharpShapeExporter exporter = new(image, new Size(-shape.ShapeBounds.XMin, -shape.ShapeBounds.YMin));
        compiledShape.Export(exporter);
        using MemoryStream ms = new();
        image.SaveAsPng(ms);
        Raylib_cs.Image img = Rl.LoadImageFromMemory(".png", ms.ToArray());
        Transform trans = Transform.CreateScale(0.05, 0.05) * Transform.CreateTranslate(x: shape.ShapeBounds.XMin, y: shape.ShapeBounds.YMin);
        return (new Texture2DWrapper(Rl.LoadTextureFromImage(img)), trans);
    }

    private static SwfFileData LoadSwf(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        return SwfFileData.CreateFrom(stream);
    }

    public const int MAX_TEXTURE_UPLOADS_PER_FRAME = 5;
    public const int MAX_SWF_UPLOADS_PER_FRAME = 1;
    public void FinalizeDraw()
    {
        TextureCache.UploadImages(MAX_TEXTURE_UPLOADS_PER_FRAME);
        SwfFileCache.UploadSwfs(MAX_SWF_UPLOADS_PER_FRAME);

        while (DrawingQueue.Count > 0)
        {
            Action drawAction = DrawingQueue.PopMin();
            drawAction();
        }
    }
}