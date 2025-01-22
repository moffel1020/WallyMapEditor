using System;
using Raylib_cs;

namespace WallyMapEditor;

public class Texture2DWrapper : IDisposable
{
    private bool _disposedValue = false;

    public Texture2D Texture { get; private init; }
    public double Width { get; private init; }
    public double Height { get; private init; }
    public WmsTransform Transform { get; private init; } = WmsTransform.IDENTITY;

    public Texture2DWrapper(Texture2D texture)
    {
        Texture = texture;
        Rl.SetTextureWrap(texture, TextureWrap.Clamp);
        Width = texture.Width;
        Height = texture.Height;
    }

    public Texture2DWrapper(Texture2D texture, WmsTransform transform) : this(texture)
    {
        Transform = transform;
    }

    ~Texture2DWrapper()
    {
        if (Texture.Id != 0)
        {
            Rl.TraceLog(TraceLogLevel.Fatal, "Finalizer called on an unfreed texture. You have a memory leak!");
        }
    }

    public static Texture2DWrapper Default => new(new() { Id = 0 });


    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (Texture.Id != 0)
            {
                Rl.UnloadTexture(Texture);
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}