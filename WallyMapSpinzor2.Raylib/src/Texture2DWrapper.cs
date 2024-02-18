using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class Texture2DWrapper : ITexture
{
    public Texture2D Texture { get; set; }

    public Texture2DWrapper(Texture2D texture)
    {
        Texture = texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
    }

    ~Texture2DWrapper()
    {
        if (Texture.Id != 0)
        {
            Rl.UnloadTexture(Texture);
        }
    }

    public static Texture2DWrapper Default => new(new() { Id = 0 });

    public int W => Texture.Width;

    public int H => Texture.Height;
}