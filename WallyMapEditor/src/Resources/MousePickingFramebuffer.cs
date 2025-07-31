using System;
using System.Collections.Generic;
using System.Numerics;
using WallyMapSpinzor2;
using Raylib_cs;

namespace WallyMapEditor;

public sealed class MousePickingFramebuffer : IDisposable
{
    private bool _disposedValue = false;

    private RenderTexture2D _framebuffer;
    public RenderTexture2D Framebuffer { get => _framebuffer; set => _framebuffer = value; }
    public Shader Shader { get; set; }
    public int Width => Framebuffer.Texture.Width;
    public int Height => Framebuffer.Texture.Height;
    private readonly List<object> _drawables = [];

    private const string FRAGMENT = @"
        #version 330

        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform float id;
        uniform sampler2D texture0;

        out float finalColor;

        void main()
        {
            vec4 texColor = texture(texture0, fragTexCoord);
            if (texColor.a <= 0.02) discard;
            finalColor = id;
        }";

    public void Load(int w, int h)
    {
        if (!Rl.IsShaderReady(Shader))
            Shader = Rl.LoadShaderFromMemory(null, FRAGMENT);

        if (_framebuffer.Id != 0)
            Rl.UnloadRenderTexture(_framebuffer);

        uint id = Rlgl.LoadFramebuffer(w, h);
        if (id == 0)
        {
            Rl.TraceLog(TraceLogLevel.Error, "Could not load object picking framebuffer");
            return;
        }

        _framebuffer.Id = id;
        Rlgl.EnableFramebuffer(_framebuffer.Id);
        unsafe { _framebuffer.Texture.Id = Rlgl.LoadTexture(null, w, h, PixelFormat.UncompressedR32, 1); }
        _framebuffer.Texture.Width = w;
        _framebuffer.Texture.Height = h;
        _framebuffer.Texture.Format = PixelFormat.UncompressedR32;
        _framebuffer.Texture.Mipmaps = 1;
        Rlgl.FramebufferAttach(_framebuffer.Id, _framebuffer.Texture.Id, FramebufferAttachType.ColorChannel0, FramebufferAttachTextureType.Texture2D, 0);
        Rlgl.DisableFramebuffer();

        if (!Rlgl.FramebufferComplete(_framebuffer.Id))
            Rl.TraceLog(TraceLogLevel.Error, "Could not complete object picking framebuffer");
    }

    public void MatchSize(ViewportWindow viewport)
    {
        if (Width != viewport.Framebuffer.Texture.Width || Height != viewport.Framebuffer.Texture.Height)
            Load(viewport.Framebuffer.Texture.Width, viewport.Framebuffer.Texture.Height);
    }

    public object? GetObjectAtCoords(ViewportWindow viewport, RaylibCanvas? canvas, IDrawable? mapData, Camera2D cam, RenderConfig config, RenderState state)
    {
        if (canvas is null || mapData is null) return null;

        MatchSize(viewport);

        Vector2 pos = Rl.GetMousePosition() - viewport.Bounds.P1;
        int posX = (int)Math.Round(pos.X);
        int posY = (int)Math.Round(pos.Y);
        int width = _framebuffer.Texture.Width;
        int height = _framebuffer.Texture.Height;
        if (posX < 0 || posY < 0 || posX >= width || posY >= height)
            return null;

        Rl.BeginTextureMode(_framebuffer);
        Rl.BeginMode2D(cam);
        Rlgl.DisableColorBlend();

        Rl.ClearBackground(RlColor.Black);
        canvas.CameraMatrix = Rl.GetCameraMatrix2D(cam);
        mapData.DrawOn(canvas, WmsTransform.IDENTITY, config, new RenderContext(), state);

        _drawables.Clear();
        while (canvas.DrawingQueue.Count > 0)
        {
            (object? obj, Action drawAction) = canvas.DrawingQueue.PopMin();
            if (obj is not null) _drawables.Add(obj);
            float id = obj is not null ? _drawables.Count : 0;

            Rl.BeginShaderMode(Shader);
            // setting shader attribs per vertex in raylib is annoying, so we do it in a hacky way with shader uniforms
            // raylib doesnt expose the integer type for framebuffer drawing so float will do 
            Rl.SetShaderValue(Shader, Rl.GetShaderLocation(Shader, "id"), id, ShaderUniformDataType.Float);
            drawAction();
            Rl.EndShaderMode();
        }

        Rlgl.EnableColorBlend();
        Rl.EndMode2D();
        Rl.EndShaderMode();
        Rl.EndTextureMode();

        float val = GetFramebufferPixel(posX, posY);

        int index = (int)Math.Round(val); // 1-indexed
        if (index <= 0 || index > _drawables.Count)
            return null;

        return _drawables[index - 1];
    }

    // warning: does not verify inputs
    private float GetFramebufferPixel(int x, int y)
    {
        Image img = default;
        try
        {
            img = Rl.LoadImageFromTexture(_framebuffer.Texture);
            unsafe { return ((float*)img.Data)[x + (img.Height - y) * img.Width]; }
        }
        finally
        {
            Rl.UnloadImage(img);
        }
    }

    public void Dispose()
    {
        if (!_disposedValue)
        {
            if (_framebuffer.Id != 0) Rl.UnloadRenderTexture(_framebuffer);
            if (Shader.Id != 0) Rl.UnloadShader(Shader);

            _disposedValue = true;
        }
        GC.SuppressFinalize(this);
    }

    ~MousePickingFramebuffer()
    {
        if (Shader.Id != 0 || _framebuffer.Texture.Id != 0)
        {
            Rl.TraceLog(TraceLogLevel.Fatal, "Finalizer called on mouse picking framebuffer. You have a memory leak!");
        }
    }
}