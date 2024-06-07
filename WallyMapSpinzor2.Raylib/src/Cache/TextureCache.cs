using System.IO;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class TextureCache : UploadCache<string, Image, Texture2DWrapper>
{
    protected override Image LoadIntermediate(string path)
    {
        path = Path.GetFullPath(path);
        return Utils.LoadRlImage(path);
    }

    protected override Texture2DWrapper IntermediateToValue(Image img)
    {
        return new(Rl.LoadTextureFromImage(img));
    }

    protected override void UnloadIntermediate(Image img)
    {
        Rl.UnloadImage(img);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }
}