using System;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class EditorLevel
{
    public Level Level { get; set; }
    public SelectionContext Selection { get; }
    public CommandHistory CommandHistory { get; }
    public OverlayManager OverlayManager { get; }
    public ILoadMethod? ReloadMethod { get; set; }
    public Camera2D Camera { get; set; } = new();

    public EditorLevel(Level level)
    {
        Level = level;
        Selection = new();
        CommandHistory = new(Selection);
        OverlayManager = new(this);
        Camera = new();
        ReloadMethod = null;
        ResetCam(Editor.INITIAL_SCREEN_WIDTH, Editor.INITIAL_SCREEN_HEIGHT);
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
    }

    public void OnSave()
    {
        CommandHistory.OnSave();
    }

    public string LevelTitle
    {
        get
        {
            string result = Level.Desc.LevelName;
            if (!CommandHistory.IsSaved) return result + '*';
            return result;
        }
    }

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