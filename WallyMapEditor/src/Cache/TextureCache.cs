using System.IO;
using Raylib_cs;

namespace WallyMapEditor;

public class TextureCache : UploadCache<string, RlImage, Texture2DWrapper>
{
    protected override RlImage LoadIntermediate(string path)
    {
        path = Path.GetFullPath(path);
        RlImage image = WmeUtils.LoadRlImage(path);
        Rl.ImageAlphaPremultiply(ref image);
        Rl.ImageMipmaps(ref image);
        return image;
    }

    protected override Texture2DWrapper IntermediateToValue(RlImage img)
    {
        Texture2D texture = Rl.LoadTextureFromImage(img);
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        Rl.GenTextureMipmaps(ref texture);
        return new(texture);
    }

    protected override void UnloadIntermediate(RlImage img)
    {
        Rl.UnloadImage(img);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }
}