using System.Numerics;
using Rl = Raylib_cs.Raylib;
using SixLabors.ImageSharp;

namespace WallyMapSpinzor2.Raylib;

public static class Utils
{
    public static Matrix4x4 TransformToMatrix(WallyMapSpinzor2.Transform t) => new(
        (float)t.ScaleX,     (float)t.SkewY,      0, 0,
        (float)t.SkewX,      (float)t.ScaleY,     0, 0,
        0,                   0,                   1, 0,
        (float)t.TranslateX, (float)t.TranslateY, 0, 1
    );

    public static Raylib_cs.Color ToRlColor(WallyMapSpinzor2.Color c) => new((int)c.R, (int)c.G, (int)c.B, (int)c.A);

    public static Raylib_cs.Texture2D LoadRlTexture(string path)
    {
        if (path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
        {
            using Image image = Image.Load(path);
            using MemoryStream ms = new();
            image.SaveAsPng(ms);

            byte[] pngBytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(pngBytes);
            Raylib_cs.Image img = Rl.LoadImageFromMemory(".png", pngBytes);
            return Rl.LoadTextureFromImage(img);
        }

        return Rl.LoadTexture(path);
    }
}
