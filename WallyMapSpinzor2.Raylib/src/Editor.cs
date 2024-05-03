using System;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace WallyMapSpinzor2.Raylib;

public class Editor(string brawlPath, string dumpPath, string fileName)
{
    public const float ZOOM_INCREMENT = 0.15f;
    public const float MIN_ZOOM = 0.01f;
    public const float MAX_ZOOM = 5.0f;
    public const float LINE_WIDTH = 5; // width at Camera zoom = 1
    public const int INITIAL_SCREEN_WIDTH = 800;
    public const int INITIAL_SCREEN_HEIGHT = 480;

    public IDrawable? MapData { get; set; }
    public string BrawlPath { get; set; } = brawlPath;

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

    private readonly RenderConfig _config = new() { };
    private double _renderSpeed = 1;

    public MousePickingFramebuffer PickingFramebuffer { get; set; } = new();

    public void LoadMap()
    {
        _selectedObject = null;
        CommandHistory.Clear();
        LevelDesc ld = Utils.DeserializeFromPath<LevelDesc>(Path.Combine(dumpPath, "Dynamic", fileName));
        LevelTypes lt = Utils.DeserializeFromPath<LevelTypes>(Path.Combine(dumpPath, "Init", "LevelTypes.xml"));
        LevelSetTypes lst = Utils.DeserializeFromPath<LevelSetTypes>(Path.Combine(dumpPath, "Game", "LevelSetTypes.xml"));
        MapData = new Level(ld, lt, lst);
    }

    public void LoadMap(string ldPath, string? ltPath, string? lstPath)
    {
        LevelDesc ld = Utils.DeserializeFromPath<LevelDesc>(ldPath);
        LevelTypes lt = ltPath is null ? new() { Levels = [] } : Utils.DeserializeFromPath<LevelTypes>(ltPath);
        LevelSetTypes lst = lstPath is null ? new() { Playlists = [] } : Utils.DeserializeFromPath<LevelSetTypes>(lstPath);

        // scuffed xml parse error handling
        if (ld.CameraBounds is null) throw new System.Xml.XmlException("LevelDesc xml did not contain essential elements");

        _selectedObject = null;
        CommandHistory.Clear();
        Canvas?.TextureCache.Clear();
        Canvas?.SwfFileCache.Clear();
        Canvas?.SwfTextureCache.Clear();
        CommandHistory.Clear();

        Level l = new(ld, lt, lst);
        l.Type ??= DefaultLevelType;
        MapData = l;
        // it's fine if there are no playlists here, they will be selected when exporting

        ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
    }

    public void LoadMap(Level l)
    {
        _selectedObject = null;
        CommandHistory.Clear();
        Canvas?.TextureCache.Clear();
        Canvas?.SwfFileCache.Clear();
        Canvas?.SwfTextureCache.Clear();
        CommandHistory.Clear();

        MapData = l;
        ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
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

    private unsafe static sbyte* ImGuiGetClipText(IntPtr userData) => Rl.GetClipboardText();
    private unsafe static void ImGuiSetClipText(IntPtr userData, sbyte* text) => Rl.SetClipboardText(text);

    public void Run()
    {
        LoadMap();

        Rl.SetConfigFlags(ConfigFlags.VSyncHint);
        Rl.InitWindow(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT, "WallyMapSpinzor2.Raylib");
        Rl.SetWindowState(ConfigFlags.ResizableWindow);
        rlImGui.Setup(true, true);
        Style.Apply();

        /*
        During rlImGui.Setup, GetClipboardText and SetClipboardText are created, and then their function ptr is used for the native callback.
        However, the library does not keep a reference to them, so those delegates get disposed, causing copy/paste to possibly crash.
        This is a hacky re-implementation of that part of the code, with the references being kept.
        */
        unsafe
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(ImGuiGetClipText);
            io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(ImGuiSetClipText);
        }

        ResetCam(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);
        PickingFramebuffer.Load(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);

        while (!Rl.WindowShouldClose())
        {
            Time += TimeSpan.FromSeconds(_renderSpeed * Rl.GetFrameTime());
            Draw();
            Update();
        }

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
        Canvas ??= new(BrawlPath);
        Canvas.CameraMatrix = Rl.GetCameraMatrix2D(_cam);
        MapData?.DrawOn(Canvas, _config, Transform.IDENTITY, Time, new RenderData());
        Canvas.FinalizeDraw();

        Rl.EndMode2D();
        Rl.EndTextureMode();

        rlImGui.End();
        Rl.EndDrawing();
    }

    private void Gui()
    {
        ImGui.DockSpaceOverViewport();
        ShowMainMenuBar();

        if (ViewportWindow.Open) ViewportWindow.Show();
        if (RenderConfigWindow.Open) RenderConfigWindow.Show(_config, ref _renderSpeed);
        if (MapOverviewWindow.Open && MapData is Level l) MapOverviewWindow.Show(l, CommandHistory, ref _selectedObject);
        if (PropertiesWindow.Open && _selectedObject is not null) PropertiesWindow.Show(_selectedObject, CommandHistory);
        if (HistoryPanel.Open) HistoryPanel.Show(CommandHistory);

        DialogWindows.RemoveAll(dialog => dialog.Closed);
        foreach (IDialog d in DialogWindows)
            d.Show();
    }

    private void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Export")) DialogWindows.Add(new ExportDialog(MapData));
            if (ImGui.MenuItem("Import")) DialogWindows.Add(new ImportDialog(this, BrawlPath));
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
                Canvas?.TextureCache.Clear();
                Canvas?.SwfTextureCache.Clear();
                Canvas?.SwfFileCache.Clear();
            }
            if (ImGui.MenuItem("Reload Map", "Ctrl+R")) LoadMap();
            if (ImGui.MenuItem("Center Camera", "R")) ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    private void Update()
    {
        if (ViewportWindow.Hovered)
        {
            float wheel = Rl.GetMouseWheelMove();
            if (wheel != 0)
            {
                _cam.Target = ScreenToWorld(Rl.GetMousePosition());
                _cam.Offset = Rl.GetMousePosition() - ViewportWindow.Bounds.P1;
                _cam.Zoom = Math.Clamp(_cam.Zoom + wheel * ZOOM_INCREMENT * _cam.Zoom, MIN_ZOOM, MAX_ZOOM);
            }

            if (Rl.IsMouseButtonPressed(MouseButton.Left))
            {
                _selectedObject = PickingFramebuffer.GetObjectAtCoords(ViewportWindow, Canvas, MapData, _config, _cam, Time);
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
            if (Rl.IsKeyPressed(KeyboardKey.R) && !Rl.IsKeyDown(KeyboardKey.LeftControl))
                ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
        }

        if (Rl.IsKeyDown(KeyboardKey.LeftControl))
        {
            if (Rl.IsKeyPressed(KeyboardKey.Z)) CommandHistory.Undo();
            if (Rl.IsKeyPressed(KeyboardKey.Y)) CommandHistory.Redo();
            if (Rl.IsKeyPressed(KeyboardKey.R)) LoadMap();
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

    ~Editor()
    {
        rlImGui.Shutdown();
        Rl.CloseWindow();
    }
}