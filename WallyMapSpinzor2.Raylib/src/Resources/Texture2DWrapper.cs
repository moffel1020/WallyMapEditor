using System;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class Texture2DWrapper : IDisposable
{
    private bool _disposedValue = false;

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

    public Texture2DWrapper(Texture2D texture, double offsetX, double offsetY, double animScale) : this(texture, offsetX * animScale, offsetY * animScale, texture.Width * animScale, texture.Height * animScale)
    {

    }

    ~Texture2DWrapper()
    {
        if (Texture.Id != 0)
        {
            Rl.TraceLog(TraceLogLevel.Fatal, "Finalizer called on an unfreed texture. You have a memory leak!");
        }
    }

    public static Texture2DWrapper Default => new(new() { Id = 0 });

    public int W => (int)Width;

    public int H => (int)Height;

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