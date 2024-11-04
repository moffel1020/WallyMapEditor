using System;
using System.Numerics;
using WallyMapSpinzor2;
using Raylib_cs;

namespace WallyMapEditor;

public class RaylibCanvas : ICanvas
{
    public BucketPriorityQueue<(object?, Action)> DrawingQueue { get; } = new(Enum.GetValues<DrawPriorityEnum>().Length);
    public AssetLoader Loader { get; }
    public RaylibAnimator Animator { get; }
    public Matrix4x4 CameraMatrix { get; set; } = Matrix4x4.Identity;

    public RaylibCanvas(AssetLoader loader)
    {
        Loader = loader;
        Animator = new(this, Loader);
    }

    public void ClearTextureCache()
    {
        Loader.ClearCache();
        Animator.ClearCache();
    }

    public void DrawCircle(double x, double y, double radius, WmsColor color, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        // FIXME: doesn't account for transformations affecting radius (could be turned into an ellipse)
        (x, y) = trans * (x, y);

        DrawingQueue.Push((caller, () =>
        {
            Rl.DrawCircleV(new((float)x, (float)y), (float)radius, WmeUtils.WmsColorToRlColor(color));
        }
        ), (int)priority);
    }

    public void DrawLine(double x1, double y1, double x2, double y2, WmsColor color, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        (x1, y1) = trans * (x1, y1);
        (x2, y2) = trans * (x2, y2);

        DrawingQueue.Push((caller, () =>
        {
            Rl.DrawLineV(new((float)x1, (float)y1), new((float)x2, (float)y2), WmeUtils.WmsColorToRlColor(color));
        }
        ), (int)priority);
    }

    public void DrawLineMultiColor(double x1, double y1, double x2, double y2, WmsColor[] colors, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        if (!Matrix4x4.Invert(CameraMatrix, out Matrix4x4 invertedMat))
            throw new ArgumentException("Camera transform is not invertible");
        WmsTransform cam = WmeUtils.Matrix4x4ToTransform(CameraMatrix);
        WmsTransform inv = WmeUtils.Matrix4x4ToTransform(invertedMat);

        (x1, y1) = cam * trans * (x1, y1);
        (x2, y2) = cam * trans * (x2, y2);
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        double center = (colors.Length - 1) / 2.0;
        float baseOffset = Rlgl.GetLineWidth();
        (double offX, double offY) = (y1 - y2, x2 - x1);
        (offX, offY) = BrawlhallaMath.Normalize(offX, offY);
        for (int i = 0; i < colors.Length; ++i)
        {
            double mult = baseOffset * (i - center);
            DrawLine(x1 + offX * mult, y1 + offY * mult, x2 + offX * mult, y2 + offY * mult, colors[i], inv, priority, caller);
        }
    }

    public void DrawRect(double x, double y, double w, double h, bool filled, WmsColor color, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        DrawingQueue.Push((caller, () =>
        {
            if (filled)
            {
                DrawRectWithTransform(x, y, w, h, trans, color);
            }
            else
            {
                DrawLine(x, y, x + w, y, color, trans, priority, caller);
                DrawLine(x + w, y, x + w, y + h, color, trans, priority, caller);
                DrawLine(x + w, y + h, x, y + h, color, trans, priority, caller);
                DrawLine(x, y + h, x, y, color, trans, priority, caller);
            }
        }
        ), (int)priority);
    }

    public void DrawString(double x, double y, string text, double fontSize, WmsColor color, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {

    }

    public void DrawTexture(string path, double x, double y, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        Texture2DWrapper texture = Loader.LoadTextureFromPath(path);
        DrawingQueue.Push((caller, () =>
        {
            DrawTextureWithTransform(texture.Texture, x + texture.XOff, y + texture.YOff, texture.W, texture.H, trans);
        }
        ), (int)priority);
    }

    public void DrawTextureRect(string path, double x, double y, double? w, double? h, WmsTransform trans, DrawPriorityEnum priority, object? caller)
    {
        Texture2DWrapper texture = Loader.LoadTextureFromPath(path);
        w ??= texture.Texture.Width;
        h ??= texture.Texture.Height;
        DrawingQueue.Push((caller, () =>
        {
            DrawTextureWithTransform(texture.Texture, x + texture.XOff, y + texture.YOff, w.Value, h.Value, trans);
        }
        ), (int)priority);
    }

    public void DrawAnim(Gfx gfx, string animName, int frame, WmsTransform trans, DrawPriorityEnum priority, object? caller, uint? loopLimit = null)
    {
        Animator.DrawAnim(gfx, animName, frame, trans, priority, caller, loopLimit);
    }

    public uint? GetAnimationFrameCount(Gfx gfx, string animName)
    {
        return Animator.GetAnimationFrameCount(gfx, animName);
    }

    public static void DrawTextureWithTransform(Texture2D texture, double x, double y, double w, double h, WmsTransform trans, float tintR = 1, float tintG = 1, float tintB = 1, float tintA = 1)
    {
        Rl.BeginBlendMode(BlendMode.AlphaPremultiply);
        Rlgl.SetTexture(texture.Id);
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4f(tintR * tintA, tintG * tintA, tintB * tintA, tintA);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] texCoords = [(0, 0), (0, 1), (1, 1), (1, 0), (0, 0)];
        (double, double)[] points = [trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin)];
        // raylib requires that the points be in counterclockwise order
        if (WmeUtils.IsPolygonClockwise(points))
        {
            Array.Reverse(texCoords);
            Array.Reverse(points);
        }
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.TexCoord2f((float)texCoords[i].Item1, (float)texCoords[i].Item2);
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.TexCoord2f((float)texCoords[i + 1].Item1, (float)texCoords[i + 1].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
        Rlgl.SetTexture(0);
        Rl.EndBlendMode();
    }

    public static void DrawRectWithTransform(double x, double y, double w, double h, WmsTransform trans, WmsColor color)
    {
        Rlgl.Begin(DrawMode.Quads);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        (double xMin, double yMin) = (x, y);
        (double xMax, double yMax) = (x + w, y + h);
        (double, double)[] points = [trans * (xMin, yMin), trans * (xMin, yMax), trans * (xMax, yMax), trans * (xMax, yMin), trans * (xMin, yMin)];
        // raylib requires that the points be in counterclockwise order
        if (WmeUtils.IsPolygonClockwise(points))
        {
            Array.Reverse(points);
        }
        for (int i = 0; i < points.Length - 1; ++i)
        {
            Rlgl.Vertex2f((float)points[i].Item1, (float)points[i].Item2);
            Rlgl.Vertex2f((float)points[i + 1].Item1, (float)points[i + 1].Item2);
        }
        Rlgl.End();
    }

    public void FinalizeDraw()
    {
        Loader.Upload();

        while (DrawingQueue.Count > 0)
        {
            (_, Action drawAction) = DrawingQueue.PopMin();
            drawAction();
        }
    }
}