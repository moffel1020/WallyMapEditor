using System;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;

using WallyMapSpinzor2;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public class Editor
{
    public const float ZOOM_INCREMENT = 0.15f;
    public const float MIN_ZOOM = 0.01f;
    public const float MAX_ZOOM = 5.0f;
    public const float LINE_WIDTH = 5; // width at Camera zoom = 1
    public const int INITIAL_SCREEN_WIDTH = 800;
    public const int INITIAL_SCREEN_HEIGHT = 480;

    public WindowTitleBar TitleBar { get; set; } = new();

    PathPreferences PathPrefs { get; }
    RenderConfigDefault ConfigDefault { get; }

    public Level? Level { get; set; }
    public LevelLoader LevelLoader { get; set; }

    public RaylibCanvas? Canvas { get; set; }
    public AssetLoader? AssetLoader { get; set; }
    private Camera2D _cam = new();
    public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(0);

    public ViewportWindow ViewportWindow { get; set; } = new();
    public RenderConfigWindow RenderConfigWindow { get; set; } = new();
    public MapOverviewWindow MapOverviewWindow { get; set; } = new();
    public PropertiesWindow PropertiesWindow { get; set; } = new();
    public ExportWindow ExportDialog { get; set; }
    public ImportWindow ImportDialog { get; set; }

    public OverlayManager OverlayManager { get; set; } = new();
    public SelectionContext Selection { get; set; } = new();
    public CommandHistory CommandHistory { get; set; }

    private readonly RenderConfig _renderConfig = RenderConfig.Default;
    private readonly OverlayConfig _overlayConfig = OverlayConfig.Default;
    private readonly RenderState _state = new();
    private RenderContext _context = new();

    public MousePickingFramebuffer PickingFramebuffer { get; set; } = new();

    private bool _showMainMenuBar = true;

    public Editor(PathPreferences pathPrefs, RenderConfigDefault configDefault) =>
        (PathPrefs, ConfigDefault, CommandHistory, ExportDialog, ImportDialog, LevelLoader) =
            (pathPrefs, configDefault, new(Selection), new(pathPrefs), new(pathPrefs), new(this));

    private OverlayData OverlayData => new()
    {
        Viewport = ViewportWindow,
        Cam = _cam,
        Context = _context,
        RenderConfig = _renderConfig,
        OverlayConfig = _overlayConfig,
        Level = Level,
    };

    private PropertiesWindowData PropertiesWindowData => new()
    {
        Time = Time,
        Canvas = Canvas,
        Loader = AssetLoader,
        Level = Level,
        PathPrefs = PathPrefs,
        Selection = Selection,
        PowerNames = LevelLoader.PowerNames,
    };

    public void Run()
    {
        Setup();

        while (!Rl.WindowShouldClose())
        {
            float delta = Rl.GetFrameTime();
            _renderConfig.Time += TimeSpan.FromSeconds(_renderConfig.RenderSpeed * delta);
            Time += TimeSpan.FromSeconds(delta);
            Draw();
            Update();
        }

        PathPrefs.Save();
        ConfigDefault.Save();
        Rl.CloseWindow();
    }

    private void Setup()
    {
#if DEBUG
        Rl.SetTraceLogLevel(TraceLogLevel.All);
#else
        Rl.SetTraceLogLevel(TraceLogLevel.Warning);
#endif

        _renderConfig.Deserialize(ConfigDefault.SerializeToXElement());

        ImportDialog.Open = true;

        Rl.SetConfigFlags(ConfigFlags.VSyncHint);
        Rl.SetConfigFlags(ConfigFlags.ResizableWindow);
        Rl.InitWindow(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT, WindowTitleBar.WINDOW_NAME);
        Rl.SetExitKey(KeyboardKey.Null);
        rlImGui.Setup(true, true);
        Style.Apply();

        ResetCam(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);
        PickingFramebuffer.Load(INITIAL_SCREEN_WIDTH, INITIAL_SCREEN_HEIGHT);

        CommandHistory.Changed += (_, _) =>
        {
            if (TitleBar.OpenLevelFile is not null)
                TitleBar.SetTitle(TitleBar.OpenLevelFile, true);
        };
    }

    private void Draw()
    {
        Rl.BeginDrawing();
        Rl.ClearBackground(RlColor.Black);
        Rlgl.SetLineWidth(Math.Max(LINE_WIDTH * _cam.Zoom, 1));
        rlImGui.Begin();
        ImGui.PushFont(Style.Font);

        Gui();

        Rl.BeginTextureMode(ViewportWindow.Framebuffer);
        Rl.BeginMode2D(_cam);

        Rl.ClearBackground(RlColor.Black);
        if (PathPrefs.BrawlhallaPath is not null)
        {
            AssetLoader ??= new(PathPrefs.BrawlhallaPath, LevelLoader.BoneTypes!);
            Canvas ??= new(AssetLoader);
            Canvas.CameraMatrix = Rl.GetCameraMatrix2D(_cam);

            _context = new();
            Level?.DrawOn(Canvas, WmsTransform.IDENTITY, _renderConfig, _context, _state);
            Canvas.FinalizeDraw();
        }

        OverlayManager.Draw(OverlayData);

        Rl.EndMode2D();
        Rl.EndTextureMode();

        ImGui.PopFont();
        rlImGui.End();
        Rl.EndDrawing();
    }

    private void Gui()
    {
        ImGui.DockSpaceOverViewport();
        if (_showMainMenuBar)
            ShowMainMenuBar();

        if (ViewportWindow.Open)
            ViewportWindow.Show();
        if (RenderConfigWindow.Open)
            RenderConfigWindow.Show(_renderConfig, ConfigDefault, PathPrefs);
        if (MapOverviewWindow.Open && Level is not null)
            MapOverviewWindow.Show(Level, CommandHistory, PathPrefs, AssetLoader, Selection);

        if (Selection.Object is not null)
            PropertiesWindow.Open = true;
        if (PropertiesWindow.Open && Selection.Object is not null)
            PropertiesWindow.Show(Selection.Object, CommandHistory, PropertiesWindowData);
        if (!PropertiesWindow.Open)
            Selection.Object = null;

        if (HistoryPanel.Open)
            HistoryPanel.Show(CommandHistory);
        if (PlaylistEditPanel.Open && Level is not null)
            PlaylistEditPanel.Show(Level, PathPrefs);
        if (KeyFinderPanel.Open)
            KeyFinderPanel.Show(PathPrefs);

        if (ExportDialog.Open)
            ExportDialog.Show(Level);
        if (ImportDialog.Open)
            ImportDialog.Show(LevelLoader);

        if (ViewportWindow.Hovered && (Rl.IsKeyPressed(KeyboardKey.Space) || Rl.IsMouseButtonPressed(MouseButton.Middle)))
        {
            AddObjectPopup.Open();
            AddObjectPopup.NewPos = ViewportWindow.ScreenToWorld(Rl.GetMousePosition(), _cam);
        }

        if (Level is not null)
            AddObjectPopup.Update(Level, CommandHistory, Selection);

        NewLevelModal.Update(LevelLoader, PathPrefs);
    }

    private bool EnableNewAndOpenMapButtons => LevelLoader.BoneTypes is not null;
    private bool EnableSaveButton => Level is not null;
    private bool EnableReloadMapButton => LevelLoader.CanReImport;
    private bool EnableCloseMapButton => Level is not null;

    private void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            bool btIsNull = LevelLoader.BoneTypes is null;
            ImGui.BeginGroup();
            ImGuiExt.WithDisabled(!EnableNewAndOpenMapButtons, () =>
            {
                if (ImGui.MenuItem("New", "Ctrl+N")) NewLevelModal.Open();
                ImGui.Separator();
                if (ImGui.MenuItem("Open", "Ctrl+O")) OpenLevelFile();
            });
            ImGui.EndGroup();
            if (btIsNull && ImGui.IsItemHovered())
                ImGui.SetTooltip("Required files need to be imported first.\nPress \"Load required files only\" in the import menu or override the individual files manually.");

            if (ImGuiExt.WithDisabledMenuItem(!EnableSaveButton, "Save", "Ctrl+S")) SaveLevelFile();
            if (ImGuiExt.WithDisabledMenuItem(!EnableSaveButton, "Save As...", "Ctrl+Shift+S")) SaveLevelFileToPath();
            ImGui.Separator();
            if (ImGui.MenuItem("Import", "Ctrl+Shift+I")) ImportDialog = new(PathPrefs) { Open = true };
            if (ImGui.MenuItem("Export", "Ctrl+Shift+E")) ExportDialog = new(PathPrefs) { Open = true };
            ImGui.Separator();
            if (ImGuiExt.WithDisabledMenuItem(!EnableReloadMapButton, "Reload map", "Ctrl+Shift+R")) ReloadMap();
            ImGui.Separator();
            if (ImGuiExt.WithDisabledMenuItem(!EnableCloseMapButton, "Close", "Ctrl+Shift+W")) CloseCurrentLevel();
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", "Ctrl+Z")) CommandHistory.Undo();
            if (ImGui.MenuItem("Redo", "Ctrl+Y")) CommandHistory.Redo();
            if (ImGui.MenuItem("Deselect", "Ctrl+D")) Selection.Object = null;
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
            if (ImGui.MenuItem("Save image", "P")) ExportWorldImage();
            if (ImGui.MenuItem("Center Camera", "R")) ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
            if (ImGui.MenuItem("History", null, HistoryPanel.Open)) HistoryPanel.Open = !HistoryPanel.Open;
            if (ImGui.MenuItem("Clear Cache")) Canvas?.ClearTextureCache();
            if (ImGui.MenuItem("Find swz key")) KeyFinderPanel.Open = !KeyFinderPanel.Open;
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
                _cam.Target = ViewportWindow.ScreenToWorld(Rl.GetMousePosition(), _cam);
                _cam.Offset = Rl.GetMousePosition() - ViewportWindow.Bounds.P1;
                _cam.Zoom = Math.Clamp(_cam.Zoom + wheel * ZOOM_INCREMENT * _cam.Zoom, MIN_ZOOM, MAX_ZOOM);
            }

            if (Rl.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 delta = Rl.GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _cam.Zoom);
                _cam.Target += delta;
            }

            if (!wantCaptureKeyboard && Rl.IsKeyPressed(KeyboardKey.R) && !Rl.IsKeyDown(KeyboardKey.LeftControl))
                ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);
        }

        bool usingOverlay = OverlayManager.IsUsing;
        OverlayManager.Update(Selection, OverlayData, CommandHistory);
        usingOverlay |= OverlayManager.IsUsing;

        if (ViewportWindow.Hovered && !usingOverlay && Rl.IsMouseButtonReleased(MouseButton.Left))
            Selection.Object = PickingFramebuffer.GetObjectAtCoords(ViewportWindow, Canvas, Level, _cam, _renderConfig, _state);

        if (!wantCaptureKeyboard && Rl.IsKeyDown(KeyboardKey.LeftControl))
        {
            if (Rl.IsKeyPressed(KeyboardKey.Z)) CommandHistory.Undo();
            if (Rl.IsKeyPressed(KeyboardKey.Y)) CommandHistory.Redo();
            if (Rl.IsKeyPressed(KeyboardKey.D)) Selection.Object = null;
            if (EnableNewAndOpenMapButtons)
            {
                if (Rl.IsKeyPressed(KeyboardKey.N)) NewLevelModal.Open();
                if (Rl.IsKeyPressed(KeyboardKey.O)) OpenLevelFile();
            }
            if (EnableSaveButton && Rl.IsKeyPressed(KeyboardKey.S)) SaveLevelFile();

            if (Rl.IsKeyDown(KeyboardKey.LeftShift) || Rl.IsKeyDown(KeyboardKey.RightShift))
            {
                if (Rl.IsKeyPressed(KeyboardKey.I)) ImportDialog = new(PathPrefs) { Open = true };
                if (Rl.IsKeyPressed(KeyboardKey.E)) ExportDialog = new(PathPrefs) { Open = true };
                if (EnableSaveButton && Rl.IsKeyPressed(KeyboardKey.S)) SaveLevelFileToPath();
                if (EnableCloseMapButton && Rl.IsKeyPressed(KeyboardKey.W)) CloseCurrentLevel();
                if (EnableReloadMapButton && Rl.IsKeyPressed(KeyboardKey.R)) ReloadMap();
            }
        }

        if (!wantCaptureKeyboard)
        {
            if (Rl.IsKeyPressed(KeyboardKey.F11)) Rl.ToggleFullscreen();
            if (Rl.IsKeyPressed(KeyboardKey.F1)) _showMainMenuBar = !_showMainMenuBar;
        }

        if (!wantCaptureKeyboard && Rl.IsKeyPressed(KeyboardKey.P))
            ExportWorldImage();
    }

    public Vector2 ScreenToWorld(Vector2 screenPos) =>
        Rl.GetScreenToWorld2D(screenPos - ViewportWindow.Bounds.P1, _cam);

    public void ResetCam() => ResetCam((int)ViewportWindow.Bounds.Width, (int)ViewportWindow.Bounds.Height);

    public void ResetCam(int surfaceW, int surfaceH)
    {
        _cam.Zoom = 1.0f;
        CameraBounds? bounds = Level?.Desc.CameraBounds;
        if (bounds is null) return;

        double scale = Math.Min(surfaceW / bounds.W, surfaceH / bounds.H);
        _cam.Offset = new(0);
        _cam.Target = new((float)bounds.X, (float)bounds.Y);
        _cam.Zoom = (float)scale;
    }

    public void ExportWorldImage()
    {
        if (Level is null || Canvas is null) return;

        Image image = GetWorldRect((float)Level.Desc.CameraBounds.X, (float)Level.Desc.CameraBounds.Y, (int)Level.Desc.CameraBounds.W, (int)Level.Desc.CameraBounds.H);
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

    public Image GetWorldRect(float x, float y, int w, int h)
    {
        if (Canvas is null)
            throw new InvalidOperationException("Cannot get world rect when Canvas is not initialized");
        RenderTexture2D renderTexture = Rl.LoadRenderTexture(w, h);
        Camera2D camera = new(new(0, 0), new(x, y), 0, 1);
        Rlgl.SetLineWidth(Math.Max(LINE_WIDTH * camera.Zoom, 1));
        Rl.BeginTextureMode(renderTexture);
        Rl.ClearBackground(RlColor.Blank);
        Rl.BeginMode2D(camera);
        Canvas.CameraMatrix = Rl.GetCameraMatrix2D(camera);
        Level?.DrawOn(Canvas, WmsTransform.IDENTITY, _renderConfig, new RenderContext(), _state);
        Canvas.FinalizeDraw();
        Rl.EndMode2D();
        Rl.EndTextureMode();
        Image image = Rl.LoadImageFromTexture(renderTexture.Texture);
        Rl.UnloadRenderTexture(renderTexture);
        return image;
    }

    public void ResetRenderState() => _state.Reset();

    private void OpenLevelFile()
    {
        Task.Run(() =>
        {
            DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(PathPrefs.LevelPath));
            if (result.IsOk)
            {
                try
                {
                    LevelLoader.LoadMap(new LevelPathLoad(result.Path));
                    PathPrefs.LevelPath = result.Path;
                    TitleBar.SetTitle(result.Path, false);
                }
                catch (Exception e)
                {
                    Rl.TraceLog(TraceLogLevel.Error, e.Message);
                }
            }
        });
    }

    private void SaveLevelFile()
    {
        if (LevelLoader.ReloadMethod is LevelPathLoad lpLoad)
        {
            WmeUtils.SerializeToPath(Level!, lpLoad.Path);
            TitleBar.SetTitle(lpLoad.Path, false);
            return;
        }

        SaveLevelFileToPath();
    }

    private void SaveLevelFileToPath()
    {
        Task.Run(() =>
        {
            DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(PathPrefs.LevelPath));
            if (result.IsOk)
            {
                WmeUtils.SerializeToPath(Level!, result.Path);
                PathPrefs.LevelPath = result.Path;
                LevelLoader.ReloadMethod = new LevelPathLoad(result.Path);
                TitleBar.SetTitle(result.Path, false);
            }
        });
    }

    public void CloseCurrentLevel()
    {
        Level = null;
        TitleBar.Reset();
        ResetState();
    }

    private void ReloadMap()
    {
        Task.Run(() =>
        {
            try
            {
                LevelLoader.ReImport();
                if (LevelLoader.ReloadMethod is LevelPathLoad lpLoad)
                    TitleBar.SetTitle(lpLoad.Path, false);
            }
            catch (Exception e)
            {
                Rl.TraceLog(TraceLogLevel.Error, e.Message);
            }
        });
    }

    public void ResetState()
    {
        Selection.Object = null;
        CommandHistory.Clear();
        Canvas?.ClearTextureCache();
        CommandHistory.Clear();
        ResetRenderState();
    }

    ~Editor()
    {
        rlImGui.Shutdown();
        Rl.CloseWindow();
    }
}