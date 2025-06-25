using System;
using System.Numerics;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class EditorLevel
{
    public Level Level { get; set; }
    public SelectionContext Selection { get; } = new();
    public CommandHistory CommandHistory { get; }
    public OverlayManager OverlayManager { get; }
    public ILoadMethod? ReloadMethod { get; set; } = null;

    public Camera2D Camera { get; set; } = new(Vector2.Zero, Vector2.Zero, 0, 1);
    public bool DidCameraInit { get; set; } = false;

    public EditorLevel(Level level)
    {
        Level = level;
        CommandHistory = new(Selection);
        OverlayManager = new(this);
    }

    public void ResetState()
    {
        Selection.Object = null;

        CommandHistory.Clear();
        OnSave(); // pretend we saved
    }

    public void ResetCam(double surfaceW, double surfaceH)
    {
        Camera2D camera = Camera;

        CameraBounds bounds = Level.Desc.CameraBounds;
        double scale = Math.Min(surfaceW / bounds.W, surfaceH / bounds.H);
        camera.Offset = new(0);
        camera.Target = new((float)bounds.X, (float)bounds.Y);
        camera.Zoom = (float)scale;

        Camera = camera;

        DidCameraInit = true;
    }

    public void OnSave() => CommandHistory.OnSave();

    public string LevelTitle => Level.Desc.LevelName;
    public bool IsSaved => CommandHistory.IsSaved;
    public string? LevelTooltip
    {
        get
        {
            if (ReloadMethod is LevelPathLoad lpLoad)
                return lpLoad.Path;
            if (ReloadMethod is ModFileLoad mfLoad)
                return mfLoad.FilePath;
            return null;
        }
    }
}