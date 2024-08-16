using System;
using Raylib_cs;

namespace WallyMapEditor;

public struct RenderRect(double x, double y, double w, double h) : IDisposable
{
    public RenderTexture2D RenderTexture { get; private set; } = Rl.LoadRenderTexture((int)w, (int)h);
    public RectUtil Rect { get; private set; } = new(x, y, x + w, y + h);

    public readonly void Dispose()
    {
        Rl.UnloadRenderTexture(RenderTexture);
    }

    public void UpdateWith(double x, double y, double w, double h)
    {
        Rect = Rect.UpdateWith(new(x, y, x + w, y + h));
        if ((int)Rect.Width > RenderTexture.Texture.Width || (int)Rect.Height > RenderTexture.Texture.Height)
        {
            Rl.UnloadRenderTexture(RenderTexture);
            RenderTexture = Rl.LoadRenderTexture((int)Rect.Width, (int)Rect.Height);
        }
    }
}