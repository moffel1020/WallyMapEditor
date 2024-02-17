using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;

using Rl = Raylib_cs.Raylib;

using SixLabors.ImageSharp;

using SwfLib.Tags;
using SwfLib.Tags.DisplayListTags;

namespace WallyMapSpinzor2.Raylib;

public static class Utils
{
    //m11, m12, m13, m14
    //m21, m22, m23, m24
    //m31, m32, m33, m34
    //m41, m42, m43, m44
    public static Matrix4x4 TransformToMatrix(Transform t) => new(
        (float)t.ScaleX, (float)t.SkewY, 0, 0,
        (float)t.SkewX, (float)t.ScaleY, 0, 0,
        0, 0, 1, 0,
        (float)t.TranslateX, (float)t.TranslateY, 0, 1
    );

    public static Transform MatrixToTransform(Matrix4x4 m) => new(m.M11, m.M21, m.M12, m.M22, m.M41, m.M42);

    public static Raylib_cs.Color ToRlColor(Color c) => new(c.R, c.G, c.B, c.A);

    public static Raylib_cs.Image LoadRlImage(string path)
    {
        //brawlhalla only supports JPG, so this is fine
        if (path.EndsWith(".jpg"))
        {
            using Image image = Image.Load(path);
            using MemoryStream ms = new();
            image.SaveAsPng(ms);
            return Rl.LoadImageFromMemory(".png", ms.ToArray());
        }

        return Rl.LoadImage(path);
    }

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

    public static IEnumerable<ushort> GetShapeIds(this DefineSpriteTag tag) =>
        tag.Tags
            .OfType<PlaceObjectBaseTag>()
            .Select(place => place.CharacterID);
}
