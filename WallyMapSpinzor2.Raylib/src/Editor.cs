using System;
using System.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Linq;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;
using NativeFileDialogSharp;
using System.Threading.Tasks;

namespace WallyMapSpinzor2.Raylib;

public class Editor(PathPreferences pathPrefs, RenderConfigDefault configDefault)
{
    public const float ZOOM_INCREMENT = 0.15f;
    public const float MIN_ZOOM = 0.01f;
    public const float MAX_ZOOM = 5.0f;
    public const float LINE_WIDTH = 5; // width at Camera zoom = 1
    public const int INITIAL_SCREEN_WIDTH = 800;
    public const int INITIAL_SCREEN_HEIGHT = 480;

    public IDrawable? MapData { get; set; }
    PathPreferences PathPrefs { get; } = pathPrefs;
    RenderConfigDefault ConfigDefault { get; } = configDefault;
    public string[]? BoneNames { get; set; }

    public RaylibCanvas? Canvas { get; set; }
    private Camera2D _cam = new();
    public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(0);

    public ViewportWindow ViewportWindow { get; set; } = new();
    public RenderConfigWindow RenderConfigWindow { get; set; } = new();
    public MapOverviewWindow MapOverviewWindow { get; set; } = new();
    public PropertiesWindow PropertiesWindow { get; set; } = new();
    public HistroyPanel HistoryPanel { get; set; } = new();
    public List<IDialog> DialogWindows { get; set; } = [];

    public CommandHistory CommandHistory { get; set; } = new();
    private object? _selectedObject = null;

    private readonly RenderConfig _config = RenderConfig.Default;
    private readonly RenderState _state = new();

    public MousePickingFramebuffer PickingFramebuffer { get; set; } = new();

    public void LoadMap(string ldPath, string? ltPath, string? lstPath, string? btPath)
    {
        if (btPath is null && BoneNames is null)
            throw new Exception("Trying to load a map without a BoneTypes.xml file, and without a bone name list already loaded");
        if (btPath is not null)
        {
            using FileStream bonesFile = new(btPath, FileMode.Open, FileAccess.Read);
            BoneNames = [.. XElement.Load(bonesFile).Elements("Bone").Select(e => e.Value)];
        }
        LevelDesc ld = Utils.DeserializeFromPath<LevelDesc>(ldPath);
        LevelTypes lt = ltPath is null ? new() { Levels = [] } : Utils.DeserializeFromPath<LevelTypes>(ltPath);
        LevelSetTypes lst = lstPath is null ? new() { Playlists = [] } : Utils.DeserializeFromPath<LevelSetTypes>(lstPath);

        // scuffed xml parse error handling
        if (ld.CameraBounds is null) throw new System.Xml.XmlException("LevelDesc xml did not contain essential elements");

        _selectedObject = null;
        CommandHistory.Clear();
        if (Canvas is not null)
        {
            Canvas.BoneNames = BoneNames!;
            Canvas.ClearTextureCache();
        }

        Level l = new(ld, lt, lst);
        if (l.Type is null)
        {
            l.Type = DefaultLevelType;
            l.Type.LevelName = ld.LevelName;
        }
        MapData = l;
        // it's fine if there are no playlists here, they will be selected when exporting

        ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
        _state.Reset();
    }

    public void LoadMap(Level l, string[] boneNames)
    {
        BoneNames = boneNames;
        _selectedObject = null;
        CommandHistory.Clear();
        if (Canvas is not null)
        {
            Canvas.BoneNames = boneNames;
            Canvas.ClearTextureCache();
        }

        MapData = l;
        ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
        _state.Reset();
    }

    public static LevelType DefaultLevelType => new()
    {
        LevelName = "UnknownLevel",
        DisplayName = "Unkown Level",
        AssetName = "a_Level_Unknown",
        FileName = "Level_Wacky.swf",
        DevOnly = false,
        TestLevel = false,
        LevelID = 0,
        CrateColorA = new(120, 120, 120),
        CrateColorB = new(120, 120, 120),
        LeftKill = 500,
        RightKill = 500,
        TopKill = 500,
        BottomKill = 500,
        BGMusic = "Level09Theme", // certified banger
        ThumbnailPNGFile = "wally.jpg"
    };

    public void Run()
    {
#if DEBUG
        Rl.SetTraceLogLevel(TraceLogLevel.All);
#else
        Rl.SetTraceLogLevel(TraceLogLevel.Warning);
#endif

        _config.Deserialize(ConfigDefault.SerializeToXElement());

        if (PathPrefs.LevelDescPath is not null && PathPrefs.BoneTypesPath is not null)
        {
            LoadMap(PathPrefs.LevelDescPath, PathPrefs.LevelTypePath, PathPrefs.LevelSetTypesPath, PathPrefs.BoneTypesPath);
        }
        else
        {
            DialogWindows.Add(new ImportDialog(this, PathPrefs));
        }

        Rl.SetConfigFlags(ConfigFlags.VSyncHint);
        Rl.InitWindow(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT, "WallyMapSpinzor2.Raylib");
        Rl.SetWindowState(ConfigFlags.ResizableWindow);
        rlImGui.Setup(true, true);
        Style.Apply();

        /*
        During rlImGui.Setup, GetClipboardText and SetClipboardText are created, and then their function ptr is used for the native callback.
        However, the library does not keep a reference to them, so those delegates get disposed, causing copy/paste to possibly crash.
        This re-implements that part of the code without the issue.
        */
        unsafe
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(static (IntPtr userData) => Rl.GetClipboardText());
            io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(static (IntPtr userData, sbyte* text) => Rl.SetClipboardText(text));
        }

        ResetCam(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);
        PickingFramebuffer.Load(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);

        while (!Rl.WindowShouldClose())
        {
            _config.Time += TimeSpan.FromSeconds(_config.RenderSpeed * Rl.GetFrameTime());
            Draw();
            Update();
        }

        PathPrefs.Save();
        ConfigDefault.Save();
        Rl.CloseWindow();
    }

    private void Draw()
    {
        Rl.BeginDrawing();
        Rl.ClearBackground(Raylib_cs.Color.Black);
        Rlgl.SetLineWidth(Math.Max(LINE_WIDTH * _cam.Zoom, 1));
        rlImGui.Begin();

        Gui();

        Rl.BeginTextureMode(ViewportWindow.Framebuffer);
        Rl.BeginMode2D(_cam);

        Rl.ClearBackground(Raylib_cs.Color.Black);
        if (PathPrefs.BrawlhallaPath is not null)
        {
            Canvas ??= new(PathPrefs.BrawlhallaPath, BoneNames!);
            Canvas.CameraMatrix = Rl.GetCameraMatrix2D(_cam);

            MapData?.DrawOn(Canvas, Transform.IDENTITY, _config, new RenderContext(), _state);
            Canvas.FinalizeDraw();
        }

        Rl.EndMode2D();
        Rl.EndTextureMode();

        rlImGui.End();
        Rl.EndDrawing();
    }

    private void Gui()
    {
        ImGui.DockSpaceOverViewport();
        ShowMainMenuBar();

        if (ViewportWindow.Open)
            ViewportWindow.Show();
        if (RenderConfigWindow.Open)
            RenderConfigWindow.Show(_config, ConfigDefault, PathPrefs);
        if (MapOverviewWindow.Open && MapData is Level l)
            MapOverviewWindow.Show(l, CommandHistory, ref _selectedObject);

        if (_selectedObject is not null)
            PropertiesWindow.Open = true;
        if (PropertiesWindow.Open && _selectedObject is not null)
            PropertiesWindow.Show(_selectedObject, CommandHistory);
        if (!PropertiesWindow.Open)
            _selectedObject = null;

        if (HistoryPanel.Open)
            HistoryPanel.Show(CommandHistory);

        DialogWindows.RemoveAll(dialog => dialog.Closed);
        foreach (IDialog d in DialogWindows)
            d.Show();
    }

    private void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Export")) DialogWindows.Add(new ExportDialog(MapData, PathPrefs));
            if (ImGui.MenuItem("Import")) DialogWindows.Add(new ImportDialog(this, PathPrefs));
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", "Ctrl+Z")) CommandHistory.Undo();
            if (ImGui.MenuItem("Redo", "Ctrl+Y")) CommandHistory.Redo();
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Viewport", null, ViewportWindow.Open)) ViewportWindow.Open = !ViewportWindow.Open;
            if (ImGui.MenuItem("Render Config", null, RenderConfigWindow.Open)) RenderConfigWindow.Open = !RenderConfigWindow.Open;
            if (ImGui.MenuItem("Map Overview", null, MapOverviewWindow.Open)) MapOverviewWindow.Open = !MapOverviewWindow.Open;
            if (ImGui.MenuItem("Object Properties", null, PropertiesWindow.Open)) PropertiesWindow.Open = !PropertiesWindow.Open;
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("History", null, HistoryPanel.Open)) HistoryPanel.Open = !HistoryPanel.Open;
            if (ImGui.MenuItem("Clear Cache"))
            {
                Canvas?.ClearTextureCache();
            }
            if (ImGui.MenuItem("Reload Map", "Ctrl+R") && PathPrefs.LevelDescPath is not null && (BoneNames is not null || PathPrefs.BoneTypesPath is not null))
            {
                LoadMap(PathPrefs.LevelDescPath, PathPrefs.LevelTypePath, PathPrefs.LevelSetTypesPath, PathPrefs.BoneTypesPath);
            }
            if (ImGui.MenuItem("Center Camera", "R")) ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    private void Update()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        bool wantCaptureKeyboard = io.WantCaptureKeyboard;
        if (ViewportWindow.Hovered)
        {
            float wheel = Rl.GetMouseWheelMove();
            if (wheel != 0)
            {
                _cam.Target = ScreenToWorld(Rl.GetMousePosition());
                _cam.Offset = Rl.GetMousePosition() - ViewportWindow.Bounds.P1;
                _cam.Zoom = Math.Clamp(_cam.Zoom + wheel * ZOOM_INCREMENT * _cam.Zoom, MIN_ZOOM, MAX_ZOOM);
            }

            if (Rl.IsMouseButtonReleased(MouseButton.Left))
            {
                _selectedObject = PickingFramebuffer.GetObjectAtCoords(ViewportWindow, Canvas, MapData, _cam, _config, _state);
                // TODO: we might want a way to associate objects with their parents. 
                // for example when selecting a hard collision we probably want to get the parent dynamic collision if it exists, when selecting an asset we want the platform
            }

            if (Rl.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 delta = Rl.GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _cam.Zoom);
                _cam.Target += delta;
            }

            // R. no ctrl.
            if (!wantCaptureKeyboard && Rl.IsKeyPressed(KeyboardKey.R) && !Rl.IsKeyDown(KeyboardKey.LeftControl))
                ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
        }

        if (!wantCaptureKeyboard && Rl.IsKeyDown(KeyboardKey.LeftControl))
        {
            if (Rl.IsKeyPressed(KeyboardKey.Z)) CommandHistory.Undo();
            if (Rl.IsKeyPressed(KeyboardKey.Y)) CommandHistory.Redo();
            // if (Rl.IsKeyPressed(KeyboardKey.R)) LoadMap();
        }

        if (!wantCaptureKeyboard && Rl.IsKeyPressed(KeyboardKey.P))
        {
            if (MapData is Level l && Canvas is not null)
            {
                Image image = GetWorldRect((float)l.Desc.CameraBounds.X, (float)l.Desc.CameraBounds.Y, (int)l.Desc.CameraBounds.W, (int)l.Desc.CameraBounds.H);
                Task.Run(() =>
                {
                    string extension = "png";
                    Rl.ImageFlipVertical(ref image);
                    DialogResult dialogResult = Dialog.FileSave(extension);
                    if (dialogResult.IsOk)
                    {
                        string path = dialogResult.Path;
                        if (!Path.HasExtension(path) || Path.GetExtension(path) != extension)
                            path = Path.ChangeExtension(path, extension);
                        Rl.ExportImage(image, path);
                    }
                    Rl.UnloadImage(image);
                });
            }
        }
    }

    public Vector2 ScreenToWorld(Vector2 screenPos) =>
        Rl.GetScreenToWorld2D(screenPos - ViewportWindow.Bounds.P1, _cam);

    private void ResetCam(int surfaceW, int surfaceH)
    {
        _cam.Zoom = 1.0f;
        CameraBounds? bounds = MapData switch
        {
            LevelDesc ld => ld.CameraBounds,
            Level l => l.Desc.CameraBounds,
            _ => null
        };

        if (bounds is null) return;

        double scale = Math.Min(surfaceW / bounds.W, surfaceH / bounds.H);
        _cam.Offset = new(0);
        _cam.Target = new((float)bounds.X, (float)bounds.Y);
        _cam.Zoom = (float)scale;
    }

    public Image GetWorldRect(float x, float y, int w, int h)
    {
        if (Canvas is null)
            throw new InvalidOperationException("Cannot get world rect when Canvas is not initialized");
        RenderTexture2D renderTexture = Rl.LoadRenderTexture(w, h);
        Camera2D camera = new(new(0, 0), new(x, y), 0, 1);
        Rl.BeginDrawing();
        Rl.ClearBackground(Raylib_cs.Color.Black);
        Rlgl.SetLineWidth(Math.Max(LINE_WIDTH * camera.Zoom, 1));
        Rl.BeginTextureMode(renderTexture);
        Rl.BeginMode2D(camera);
        Canvas.CameraMatrix = Rl.GetCameraMatrix2D(camera);
        MapData?.DrawOn(Canvas, Transform.IDENTITY, _config, new RenderContext(), _state);
        Canvas.FinalizeDraw();
        Rl.EndMode2D();
        Rl.EndTextureMode();
        Rl.EndDrawing();
        Image image = Rl.LoadImageFromTexture(renderTexture.Texture);
        Rl.UnloadRenderTexture(renderTexture);
        return image;
    }

    ~Editor()
    {
        rlImGui.Shutdown();
        Rl.CloseWindow();
    }
}