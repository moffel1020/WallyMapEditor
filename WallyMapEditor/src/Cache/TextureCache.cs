using System.IO;
using Raylib_cs;
using SkiaSharp;

namespace WallyMapEditor;

public sealed class TextureCache : UploadCache<string, RlImage?, Texture2DWrapper>
{
    protected override RlImage? LoadIntermediate(string path)
    {
        path = Path.GetFullPath(path);
        using SKBitmap? bitmap = WmeUtils.LoadSKBitmap(path);
        if (bitmap is null)
            return null;

        RlImage img1 = WmeUtils.SKBitmapAsRlImage(bitmap);
        // alpha premult done in LoadSKBitmap
        RlImage img2 = RaylibEx.ImageCopyWithMipmaps(img1);
        bitmap.Dispose(); // also unloads img1

        return img2;
    }

    protected override Texture2DWrapper IntermediateToValue(RlImage? img)
    {
        if (img is null)
            return Texture2DWrapper.Default;

        Texture2D texture = Rl.LoadTextureFromImage(img.Value);
        return new(texture);
    }

    protected override void InitValue(Texture2DWrapper v)
    {
        Texture2D texture = v.Texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        Rl.GenTextureMipmaps(ref texture);
    }

    protected override void UnloadIntermediate(RlImage? img)
    {
        if (img is not null) Rl.UnloadImage(img.Value);
    }

    protected override void UnloadValue(Texture2DWrapper texture)
    {
        texture.Dispose();
    }
}