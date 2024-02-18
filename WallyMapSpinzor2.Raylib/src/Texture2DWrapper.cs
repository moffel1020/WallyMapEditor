using Raylib_cs;
using SwfLib.Data;
using SwiffCheese.Utils;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class Texture2DWrapper : ITexture
{
    public Texture2D Texture { get; private init; }
    public double XOff { get; private init; }
    public double YOff { get; private init; }
    public double Width { get; private init; }
    public double Height { get; private init; }

    public Texture2DWrapper(Texture2D texture)
    {
        Texture = texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        XOff = YOff = 0;
        Width = texture.Width;
        Height = texture.Height;
    }

    public Texture2DWrapper(Texture2D texture, double x, double y, double w, double h) : this(texture)
    {
        XOff = x;
        YOff = y;
        Width = w;
        Height = h;
    }

    public Texture2DWrapper(Texture2D texture, SwfRect rect) : this(texture, rect.XMin / 20, rect.YMin / 20, rect.Width() / 20, rect.Height() / 20)
    {

    }

    ~Texture2DWrapper()
    {
        if (Texture.Id != 0)
        {
            Rl.UnloadTexture(Texture);
        }
    }

    public static Texture2DWrapper Default => new(new() { Id = 0 });

    public int W => (int)Width;

    public int H => (int)Height;
}