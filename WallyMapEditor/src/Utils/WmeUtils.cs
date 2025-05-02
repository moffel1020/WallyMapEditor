global using Rl = Raylib_cs.Raylib;
global using RlColor = Raylib_cs.Color;
global using RlImage = Raylib_cs.Image;
global using WmsColor = WallyMapSpinzor2.Color;
global using WmsTransform = WallyMapSpinzor2.Transform;

using System;
using System.Collections.Generic;
using System.Numerics;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static partial class WmeUtils
{
    public static bool IsPolygonClockwise(IReadOnlyList<(double, double)> poly)
    {
        double area = 0;
        for (int i = 0; i < poly.Count; ++i)
        {
            int j = (i + 1) % poly.Count;
            (double x1, double y1) = poly[i];
            (double x2, double y2) = poly[j];
            area += BrawlhallaMath.Cross(x1, y1, x2, y2);
        }
        return area > 0;
    }

    public static T[] RemoveAt<T>(T[] array, int index)
    {
        T[] result = new T[array.Length - 1];
        for ((int i, int j) = (0, 0); i < array.Length; ++i)
        {
            if (i != index)
            {
                result[j] = array[i];
                ++j;
            }
        }
        return result;
    }

    public static T[] MoveUp<T>(T[] array, int index)
    {
        T[] result = [.. array];
        if (index > 0)
            (result[index], result[index - 1]) = (result[index - 1], result[index]);
        return result;
    }

    public static T[] MoveDown<T>(T[] array, int index)
    {
        T[] result = [.. array];
        if (index < array.Length - 1)
            (result[index], result[index + 1]) = (result[index + 1], result[index]);
        return result;
    }

    public static IEnumerable<U> MapFilter<T, U>(this IEnumerable<T> enumerable, Func<T, Maybe<U>> map)
    {
        foreach (T t in enumerable)
        {
            if (map(t).TryGetValue(out U? u))
                yield return u;
        }
    }

    public static void DrawArrow(double x1, double y1, double x2, double y2, double arrowSide, double arrowBack, RlColor color)
    {
        Rl.DrawLineV(new((float)x1, (float)y1), new((float)x2, (float)y2), color);
        // draw arrow parts
        // we start with an arrow pointing right
        // and we rotate it to match
        double length = BrawlhallaMath.Length(x2 - x1, y2 - y1); // arrow length
        (double dirX, double dirY) = BrawlhallaMath.Normalize(x2 - x1, y2 - y1); // arrow direction
        double angle = Math.Atan2(dirY, dirX); // arrow angle
        // calculate end points by applying the rotation to the arrow
        (double arrowEndX1, double arrowEndY1) = BrawlhallaMath.Rotated(length - arrowBack, arrowSide, angle);
        (double arrowEndX2, double arrowEndY2) = BrawlhallaMath.Rotated(length - arrowBack, -arrowSide, angle);
        // draw the lines
        (double x3, double y3) = (x1 + arrowEndX1, y1 + arrowEndY1);
        (double x4, double y4) = (x1 + arrowEndX2, y1 + arrowEndY2);
        Rl.DrawLineV(new((float)x2, (float)y2), new((float)x3, (float)y3), color);
        Rl.DrawLineV(new((float)x2, (float)y2), new((float)x4, (float)y4), color);
    }

    public static bool CheckCollisionPointRec(Vector2 point, Raylib_cs.Rectangle rec)
    {
        if (rec.Width < 0)
        {
            rec.X += rec.Width;
            rec.Width = -rec.Width;
        }
        if (rec.Height < 0)
        {
            rec.Y += rec.Height;
            rec.Height = -rec.Height;
        }

        return Rl.CheckCollisionPointRec(point, rec);
    }

    public static bool CheckCollisionPointRotatedRec(Vector2 point, Raylib_cs.Rectangle rec, double rotation, Vector2 origin)
    {
        if (rotation != 0)
        {
            Vector2 center = new(rec.X, rec.Y);
            (float sin, float cos) = ((float, float))Math.SinCos(-rotation);
            Vector2 temp;

            point -= center;
            temp.X = point.X * cos - point.Y * sin;
            temp.Y = point.X * sin + point.Y * cos;
            point = temp + center;
        }

        rec.X -= origin.X;
        rec.Y -= origin.Y;
        return CheckCollisionPointRec(point, rec);
    }

    public static IEnumerable<(T, int)> Indexed<T>(this IEnumerable<T> e)
    {
        int i = 0;
        foreach (T t in e)
            yield return (t, i++);
    }
}
