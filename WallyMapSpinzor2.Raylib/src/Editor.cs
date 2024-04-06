using System;
using System.Numerics;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;

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

    public RaylibCanvas? Canvas { get; set; }
    private Camera2D _cam = new();
    public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(0);

    public ViewportWindow ViewportWindow { get; set; } = new();
    public RenderConfigWindow RenderConfigWindow { get; set; } = new();
    public MapOverviewWindow MapOverviewWindow { get; set; } = new();
    public PropertiesWindow PropertiesWindow { get; set; } = new();
    public HistroyPanel HistoryPanel { get; set; } = new();

    public CommandHistory CommandHistory { get; set; } = new();
    private object? _selectedObject = null;

    private readonly RenderConfig _config = new()
    {

    };

    private static T DeserializeFromPath<T>(string fromPath)
    where T : IDeserializable, new()
    {
        XElement element;
        using (FileStream fromFile = new(fromPath, FileMode.Open, FileAccess.Read))
        {
            element = XElement.Load(fromFile);
        }
        return element.DeserializeTo<T>();
    }

    public void LoadMap()
    {
        LevelDesc ld = DeserializeFromPath<LevelDesc>(Path.Combine(dumpPath, "Dynamic", fileName));
        LevelTypes lt = DeserializeFromPath<LevelTypes>(Path.Combine(dumpPath, "Init", "LevelTypes.xml"));
        LevelSetTypes lst = DeserializeFromPath<LevelSetTypes>(Path.Combine(dumpPath, "Game", "LevelSetTypes.xml"));
        MapData = new Level(ld, lt, lst);
    }

    public void SaveMap()
    {
        LevelDesc? ld = MapData switch
        {
            Level l => l.Desc,
            LevelDesc d => d,
            _ => null
        };

        if (ld is null) return;

        XElement e = ld.SerializeToXElement();
        string newFileName = fileName.StartsWith("new_") ? fileName : $"new_{fileName}";
        string toPath = Path.Combine(dumpPath, "Dynamic", newFileName);
        using (FileStream toFile = new(toPath, FileMode.Create, FileAccess.Write))
        {
            using XmlWriter xmlw = XmlWriter.Create(toFile, new()
            {
                OmitXmlDeclaration = true, //no xml header
                IndentChars = "    ",
                Indent = true, //indent with four spaces
                NewLineChars = "\n", //use UNIX line endings
                Encoding = new UTF8Encoding(false) //use UTF8 (no BOM) encoding
            });
            e.Save(xmlw);
        }
        fileName = newFileName;
    }

    public void Run()
    {
        LoadMap();

        Rl.SetConfigFlags(ConfigFlags.VSyncHint);
        Rl.InitWindow(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT, "WallyMapSpinzor2.Raylib");
        Rl.SetWindowState(ConfigFlags.ResizableWindow);
        rlImGui.Setup(true, true);
        Style.Apply();

        ResetCam(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT); // inaccurate, but it will do for now

        while (!Rl.WindowShouldClose())
        {
            Time += TimeSpan.FromSeconds(Rl.GetFrameTime());
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
        Canvas ??= new(brawlPath);
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
        if (RenderConfigWindow.Open) RenderConfigWindow.Show(_config);
        if (MapOverviewWindow.Open && MapData is Level l) MapOverviewWindow.Show(l, CommandHistory, ref _selectedObject);
        if (PropertiesWindow.Open && _selectedObject is not null) PropertiesWindow.Show(_selectedObject, CommandHistory);
        if (HistoryPanel.Open) HistoryPanel.Show(CommandHistory);
    }

    private void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
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
            if (ImGui.MenuItem("Reload Map", "Ctrl+R"))
            {
                LoadMap();
            }
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Exporting"))
        {
            if (ImGui.MenuItem("Export Map"))
            {
                SaveMap();
            }
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
                Vector2 mousePos = Rl.GetScreenToWorld2D(Rl.GetMousePosition() - ViewportWindow.Bounds.P1, _cam);
                _cam.Offset = Rl.GetMousePosition() - ViewportWindow.Bounds.P1;
                _cam.Target = mousePos;
                _cam.Zoom = Math.Clamp(_cam.Zoom + wheel * ZOOM_INCREMENT * _cam.Zoom, MIN_ZOOM, MAX_ZOOM);
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