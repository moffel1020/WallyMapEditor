using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class Texture2DWrapper : ITexture
{
    public Texture2D? Texture { get; set; }

    public Texture2DWrapper(Texture2D? texture)
    {
        Texture = texture;
    }

    ~Texture2DWrapper()
    {
        if (Texture is not null) Raylib_cs.Raylib.UnloadTexture((Texture2D)Texture);
    }

    public int W => Texture?.Width ?? 0;

    public int H => Texture?.Height ?? 0;
}