using System.IO;
using Raylib_cs;
using SkiaSharp;

namespace WallyMapEditor;

public sealed class TextureCache : UploadCache<string, RlImage, Texture2DWrapper>
{
    protected override RlImage LoadIntermediate(string path)
    {
        path = Path.GetFullPath(path);
        using SKBitmap bitmap = WmeUtils.LoadSKBitmap(path);
        RlImage img1 = WmeUtils.SKBitmapAsRlImage(bitmap);
        // alpha premult done in LoadSKBitmap
        RlImage img2 = RaylibEx.ImageCopyWithMipmaps(img1);
        bitmap.Dispose(); // also unloads img1

        return img2;
    }

    protected override Texture2DWrapper IntermediateToValue(RlImage img)
    {
        Texture2D texture = Rl.LoadTextureFromImage(img);
        return new(texture);
    }

    protected override void InitValue(Texture2DWrapper v)
    {
        Texture2D texture = v.Texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        Rl.GenTextureMipmaps(ref texture);
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