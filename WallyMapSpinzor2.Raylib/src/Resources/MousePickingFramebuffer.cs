using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class MousePickingFramebuffer : IDisposable
{
    private bool _disposedValue = false;

    private RenderTexture2D _framebuffer;
    public RenderTexture2D Framebuffer { get => _framebuffer; set => _framebuffer = value; }
    public Shader Shader { get; set; }
    public int Width => Framebuffer.Texture.Width;
    public int Height => Framebuffer.Texture.Height;
    private readonly List<object> _drawables = [];

    private const string VERTEX = @"
        #version 330

        in vec3 vertexPosition;
        in vec2 vertexTexCoord;
        in vec4 vertexColor;

        out vec2 fragTexCoord;
        out vec4 fragColor;

        uniform mat4 mvp;

        void main()
        {
            fragTexCoord = vertexTexCoord;
            fragColor = vertexColor;
            gl_Position = mvp*vec4(vertexPosition, 1.0);
        };";

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
            Shader = Rl.LoadShaderFromMemory(VERTEX, FRAGMENT);

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

    public object? GetObjectAtCoords(ViewportWindow viewport, RaylibCanvas? canvas, IDrawable? mapData, RenderConfig config, Camera2D cam, TimeSpan time)
    {
        if (canvas is null || mapData is null) return null;

        MatchSize(viewport);

        Rl.BeginTextureMode(_framebuffer);
        Rl.BeginShaderMode(Shader);
        Rl.BeginMode2D(cam);
        Rlgl.DisableColorBlend();

        Rl.ClearBackground(Raylib_cs.Color.Black);
        canvas.CameraMatrix = Rl.GetCameraMatrix2D(cam);

        mapData.DrawOn(canvas, config, Transform.IDENTITY, time, new RenderData());

        _drawables.Clear();
        while (canvas.DrawingQueue.Count > 0)
        {
            (object? obj, Action drawAction) = canvas.DrawingQueue.PopMin();
            if (obj is not null) _drawables.Add(obj);
            // setting shader attribs per vertex in raylib is annoying, so we do it in a hacky way with shader uniforms
            // raylib doesnt expose the integer type for framebuffer drawing so float will do 
            Rl.SetShaderValue(Shader, Rl.GetShaderLocation(Shader, "id"), (float)_drawables.Count, ShaderUniformDataType.Float);
            drawAction();
            Rlgl.DrawRenderBatchActive(); // force drawcall so uniform value gets drawn
        }

        Rlgl.EnableColorBlend();
        Rl.EndMode2D();
        Rl.EndShaderMode();
        Rl.EndTextureMode();

        object? selected = null;
        Image img = Rl.LoadImageFromTexture(_framebuffer.Texture);

        Vector2 pos = Rl.GetMousePosition() - viewport.Bounds.P1;
        if (pos.X >= 0 && pos.X < img.Width && pos.Y >= 0 && pos.Y < img.Height && Rl.IsImageReady(img))
        {
            float val = 0;
            unsafe { val = ((float*)img.Data)[(int)pos.X + (img.Height - (int)pos.Y) * img.Width]; }

            int index = (int)Math.Round(val, 0);
            if (index > 0 && index <= _drawables.Count)
                selected = _drawables[index - 1];
        }
        Rl.UnloadImage(img);
        return selected;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (_framebuffer.Id != 0) Rl.UnloadRenderTexture(_framebuffer);
        if (Shader.Id != 0) Rl.UnloadShader(Shader);

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
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