using System;
using System.Numerics;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class Editor
{
    public const float ZOOM_INCREMENT = 0.15f;
    public const float MIN_ZOOM = 0.01f;
    public const float MAX_ZOOM = 5.0f;
    public const float LINE_WIDTH = 5; // width at Camera zoom = 1

    public string BrawlPath { get; set; }
    public RaylibCanvas? Canvas { get; set; }

    private Camera2D _cam = new();
    public float Zoom { get => _cam.Zoom; }
    public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(0);

    private IDrawable _toDraw;
    public IDrawable ToDraw
    {
        get => _toDraw;
        set
        {
            _toDraw = value;
            ResetCam();
        }
    }

    public Editor(string brawlPath, IDrawable toDraw)
    {
        BrawlPath = brawlPath;
        _toDraw = toDraw;
    }

    private readonly RenderConfig _config = new()
    {
        RedScore = 4,
        BlueScore = 11
    };

    public void Run()
    {
        Rl.SetConfigFlags(ConfigFlags.VSyncHint);
        Rl.InitWindow(800, 480, "WallyMapSpinzor2.Raylib");
        Rl.SetWindowState(ConfigFlags.ResizableWindow);
        ResetCam();

        while (!Rl.WindowShouldClose())
        {
            if (Rl.IsWindowResized()) ResetCam();

            Time += TimeSpan.FromSeconds(Rl.GetFrameTime());
            Draw(Time);
            Update(Time);
        }

        Rl.CloseWindow();
    }

    private void Draw(TimeSpan ts)
    {
        Rl.BeginDrawing();
        Rlgl.SetLineWidth(Math.Min(LINE_WIDTH * _cam.Zoom, 1));
        Rl.ClearBackground(Raylib_cs.Color.Black);
        Rl.BeginMode2D(_cam);

        Canvas ??= new(BrawlPath);
        Canvas.CameraMatrix = Rl.GetCameraMatrix2D(_cam);
        ToDraw.DrawOn(Canvas, _config, Transform.IDENTITY, ts, new RenderData());
        Canvas.FinalizeDraw();

        Rl.EndMode2D();
        Rl.EndDrawing();
    }

    private void Update(TimeSpan ts)
    {
        float wheel = Rl.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mousePos = Rl.GetScreenToWorld2D(Rl.GetMousePosition(), _cam);
            _cam.Offset = Rl.GetMousePosition();
            _cam.Target = mousePos;
            _cam.Zoom = Math.Clamp(_cam.Zoom + wheel * ZOOM_INCREMENT * _cam.Zoom, MIN_ZOOM, MAX_ZOOM);
        }

        if (Rl.IsMouseButtonDown(MouseButton.Right))
        {
            Vector2 delta = Rl.GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _cam.Zoom);
            _cam.Target += delta;
        }
    }

    private void ResetCam()
    {
        _cam.Zoom = 1.0f;
        CameraBounds? bounds = ToDraw switch
        {
            LevelDesc ld => ld.CameraBounds,
            Level l => l.Desc.CameraBounds,
            _ => null
        };

        if (bounds is null) return;

        double screenW = Rl.GetScreenWidth();
        double screenH = Rl.GetScreenHeight();

        double scale = Math.Min(screenW / bounds.W, screenH / bounds.H);
        _cam.Offset = new(0);
        _cam.Target = new((float)bounds.X, (float)bounds.Y);
        _cam.Zoom = (float)scale;
    }
}