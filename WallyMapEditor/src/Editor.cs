using System;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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
    public const int INITIAL_SCREEN_WIDTH = 1280;
    public const int INITIAL_SCREEN_HEIGHT = 720;

    public WindowTitleBar TitleBar { get; set; } = new();

    PathPreferences PathPrefs { get; }
    RenderConfigDefault ConfigDefault { get; }

    private EditorLevel? _currentLevel;
    public EditorLevel? CurrentLevel { get => _currentLevel; set => _currentLevel = value; }

    public object? SelectedObject => CurrentLevel?.Selection.Object;

    public List<EditorLevel> LoadedLevels { get; set; } = [];
    public LevelLoader LevelLoader { get; set; }

    public RaylibCanvas? Canvas { get; set; }
    public AssetLoader? AssetLoader { get; set; }
    private Camera2D _cam = new();
    private bool _camResetQueued = false;
    public TimeSpan Time { get; set; } = TimeSpan.FromSeconds(0);

    public ViewportWindow ViewportWindow { get; set; } = new();
    public RenderConfigWindow RenderConfigWindow { get; set; } = new();
    public MapOverviewWindow MapOverviewWindow { get; set; } = new();
    public PropertiesWindow PropertiesWindow { get; set; } = new();
    public ExportWindow ExportDialog { get; set; }
    public ImportWindow ImportDialog { get; set; }
    public BackupsWindow BackupsDialog { get; set; }
    public ModCreatorWindow ModCreatorDialog { get; set; }
    public ModLoaderWindow ModLoaderDialog { get; set; }

    private bool _movingObject = false;

    private bool _renderPaused = false;
    private readonly RenderConfig _renderConfig = RenderConfig.Default;
    private readonly OverlayConfig _overlayConfig = OverlayConfig.Default;
    private readonly RenderState _state = new();
    private RenderContext _context = new();

    private readonly BackupsList _backupsList = new();

    public MousePickingFramebuffer PickingFramebuffer { get; set; } = new();

    private bool _showMainMenuBar = true;

    public Editor(PathPreferences pathPrefs, RenderConfigDefault configDefault)
    {
        PathPrefs = pathPrefs;
        ConfigDefault = configDefault;
        ExportDialog = new(pathPrefs, _backupsList);
        ImportDialog = new(pathPrefs);
        BackupsDialog = new(pathPrefs, _backupsList);
        ModCreatorDialog = new(pathPrefs);
        ModLoaderDialog = new(pathPrefs);
        LevelLoader = new();
    }

    private OverlayData OverlayData => new()
    {
        Viewport = ViewportWindow,
        Cam = _cam,
        Context = _context,
        RenderConfig = _renderConfig,
        OverlayConfig = _overlayConfig,
        Level = CurrentLevel?.Level,
    };

    private PropertiesWindowData PropertiesWindowData => new()
    {
        Time = Time,
        Canvas = Canvas,
        Loader = AssetLoader,
        Level = CurrentLevel?.Level,
        Selection = CurrentLevel?.Selection,
        PathPrefs = PathPrefs,
        PowerNames = LevelLoader.PowerNames,
    };

    public void Run()
    {
        Setup();

        while (!Rl.WindowShouldClose())
        {
            float delta = Rl.GetFrameTime();
            if (!_renderPaused)
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
        LogCallback.Init();

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

        ViewportWindow.ResetCamPossible += (_) =>
        {
            if (_camResetQueued)
            {
                _camResetQueued = false;
                ResetCam();
            }
        };

        PathPrefs.BrawlhallaPathChanged += (_, path) =>
        {
            if (AssetLoader is not null)
                AssetLoader.BrawlPath = path;
        };

        LevelLoader.BoneTypesChanged += (_, boneTypes) =>
        {
            if (AssetLoader is not null)
                AssetLoader.BoneTypes = boneTypes;
        };

        LevelLoader.OnNewMapLoaded += (_, level) =>
        {
            AddNewLevel(level, true);
        };

        LevelLoader.OnMapReloaded += (_, level, newData, loadMethod) =>
        {
            OnLevelReloaded(level, newData, loadMethod);
        };

        RenderConfigWindow.MoveFrames += (_, frames) =>
        {
            _renderConfig.Time += TimeSpan.FromSeconds(frames / 60.0);
        };

        RenderConfigWindow.SetFrames += (_, frames) =>
        {
            _renderConfig.Time = TimeSpan.FromSeconds(frames / 60.0);
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
            CurrentLevel?.Level.DrawOn(Canvas, WmsTransform.IDENTITY, _renderConfig, _context, _state);
            Canvas.FinalizeDraw();
        }

        CurrentLevel?.OverlayManager.Draw(OverlayData);

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
            ViewportWindow.Show(LoadedLevels, ref _currentLevel, _camResetQueued);
        if (RenderConfigWindow.Open)
            RenderConfigWindow.Show(_renderConfig, ConfigDefault, PathPrefs, ref _renderPaused);
        if (MapOverviewWindow.Open && CurrentLevel is not null)
            MapOverviewWindow.Show(CurrentLevel, PathPrefs, AssetLoader);

        if (CurrentLevel?.Selection.Object is not null)
        {
            PropertiesWindow.Open = true;
            PropertiesWindow.Show(CurrentLevel.Selection.Object, CurrentLevel.CommandHistory, PropertiesWindowData);
        }
        if (!PropertiesWindow.Open && CurrentLevel is not null)
            CurrentLevel.Selection.Object = null;

        if (HistoryPanel.Open && CurrentLevel is not null)
            HistoryPanel.Show(CurrentLevel.CommandHistory);
        if (PlaylistEditPanel.Open && CurrentLevel is not null)
            PlaylistEditPanel.Show(CurrentLevel.Level, PathPrefs);
        if (KeyFinderPanel.Open)
            KeyFinderPanel.Show(PathPrefs);

        if (ExportDialog.Open && CurrentLevel is not null)
            ExportDialog.Show(CurrentLevel.Level);
        if (ImportDialog.Open)
            ImportDialog.Show(LevelLoader);
        if (BackupsDialog.Open)
            BackupsDialog.Show();
        if (ModCreatorDialog.Open)
            ModCreatorDialog.Show();
        if (ModLoaderDialog.Open)
            ModLoaderDialog.Show();

        if (ViewportWindow.Hovered && (Rl.IsKeyPressed(KeyboardKey.Space) || Rl.IsMouseButtonPressed(MouseButton.Middle)))
        {
            AddObjectPopup.Open();
            AddObjectPopup.NewPos = ViewportWindow.ScreenToWorld(Rl.GetMousePosition(), _cam);
        }

        if (CurrentLevel is not null)
            AddObjectPopup.Update(CurrentLevel);

        NewLevelModal.Update(LevelLoader, PathPrefs);
    }

    private bool EnableNewAndOpenMapButtons => LevelLoader.BoneTypes is not null;
    private bool EnableSaveButton => CurrentLevel is not null;
    private bool EnableReloadMapButton => LevelLoader.CanReImport(CurrentLevel);
    private bool EnableCloseMapButton => CurrentLevel is not null;

    private void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            bool btIsNull = LevelLoader.BoneTypes is null;
            ImGui.BeginGroup();
            using (ImGuiExt.DisabledIf(!EnableNewAndOpenMapButtons))
            {
                if (ImGui.MenuItem("New", "Ctrl+N")) NewLevelModal.Open();
                ImGui.Separator();
                if (ImGui.MenuItem("Open", "Ctrl+O")) OpenLevelFile();
            }
            ImGui.EndGroup();
            if (btIsNull && ImGui.IsItemHovered())
                ImGui.SetTooltip("Required files need to be imported first.\nPress \"Load required files only\" in the import menu or override the individual files manually.");

            if (ImGuiExt.MenuItemDisabledIf(!EnableSaveButton, "Save", "Ctrl+S")) SaveLevelFile();
            if (ImGuiExt.MenuItemDisabledIf(!EnableSaveButton, "Save As...", "Ctrl+Shift+S")) SaveLevelFileToPath();
            ImGui.Separator();
            if (ImGui.MenuItem("Import", "Ctrl+Shift+I")) ImportDialog = new(PathPrefs) { Open = true };
            if (ImGui.MenuItem("Export", "Ctrl+Shift+E")) ExportDialog = new(PathPrefs, _backupsList) { Open = true };
            ImGui.Separator();
            if (ImGuiExt.MenuItemDisabledIf(!EnableReloadMapButton, "Reload map", "Ctrl+Shift+R")) ReloadMap();
            ImGui.Separator();
            if (ImGuiExt.MenuItemDisabledIf(!EnableCloseMapButton, "Close", "Ctrl+Shift+W")) CloseCurrentLevel();
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            if (CurrentLevel is not null)
            {
                if (ImGui.MenuItem("Undo", "Ctrl+Z")) CurrentLevel.CommandHistory.Undo();
                if (ImGui.MenuItem("Redo", "Ctrl+Y")) CurrentLevel.CommandHistory.Redo();
                if (ImGui.MenuItem("Deselect", "Ctrl+D")) CurrentLevel.Selection.Object = null;
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.MenuItem("Undo", "Ctrl+Z");
                ImGui.MenuItem("Redo", "Ctrl+Y");
                ImGui.MenuItem("Deselect", "Ctrl+D");
                ImGui.EndDisabled();
            }
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
            if (ImGui.MenuItem("Center Camera", "R")) ResetCam();
            if (ImGui.MenuItem("History", null, HistoryPanel.Open)) HistoryPanel.Open = !HistoryPanel.Open;
            if (ImGui.MenuItem("Clear Cache")) Canvas?.ClearTextureCache();
            if (ImGui.MenuItem("Find swz key", null, KeyFinderPanel.Open)) KeyFinderPanel.Open = !KeyFinderPanel.Open;
            if (ImGui.MenuItem("Manage swz backups", null, BackupsDialog.Open)) BackupsDialog.Open = !BackupsDialog.Open;
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Mods"))
        {
            if (ImGui.MenuItem("Create mod", null, ModCreatorDialog.Open)) ModCreatorDialog.Open = !ModCreatorDialog.Open;
            if (ImGui.MenuItem("Load mods", null, ModLoaderDialog.Open)) ModLoaderDialog.Open = !ModLoaderDialog.Open;
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
                ResetCam();
        }

        if (CurrentLevel is not null)
        {
            bool usingOverlay = CurrentLevel.OverlayManager.IsUsing;
            CurrentLevel.OverlayManager.Update(OverlayData);
            usingOverlay |= CurrentLevel.OverlayManager.IsUsing;

            if (ViewportWindow.Hovered && !usingOverlay && Rl.IsMouseButtonReleased(MouseButton.Left))
                CurrentLevel.Selection.Object = PickingFramebuffer.GetObjectAtCoords(ViewportWindow, Canvas, CurrentLevel.Level, _cam, _renderConfig, _state);
        }

        if (!wantCaptureKeyboard && Rl.IsKeyDown(KeyboardKey.LeftControl))
        {
            if (CurrentLevel is not null)
            {
                if (Rl.IsKeyPressed(KeyboardKey.Z)) CurrentLevel.CommandHistory.Undo();
                if (Rl.IsKeyPressed(KeyboardKey.Y)) CurrentLevel.CommandHistory.Redo();
                if (Rl.IsKeyPressed(KeyboardKey.D)) CurrentLevel.Selection.Object = null;
            }

            if (EnableNewAndOpenMapButtons)
            {
                if (Rl.IsKeyPressed(KeyboardKey.N)) NewLevelModal.Open();
                if (Rl.IsKeyPressed(KeyboardKey.O)) OpenLevelFile();
            }
            if (EnableSaveButton && Rl.IsKeyPressed(KeyboardKey.S)) SaveLevelFile();

            if (Rl.IsKeyDown(KeyboardKey.LeftShift) || Rl.IsKeyDown(KeyboardKey.RightShift))
            {
                if (Rl.IsKeyPressed(KeyboardKey.I)) ImportDialog = new(PathPrefs) { Open = true };
                if (Rl.IsKeyPressed(KeyboardKey.E)) ExportDialog = new(PathPrefs, _backupsList) { Open = true };
                if (EnableSaveButton && Rl.IsKeyPressed(KeyboardKey.S)) SaveLevelFileToPath();
                if (EnableCloseMapButton && Rl.IsKeyPressed(KeyboardKey.W)) CloseCurrentLevel();
                if (EnableReloadMapButton && Rl.IsKeyPressed(KeyboardKey.R)) ReloadMap();
            }
        }

        bool wasMovingObject = _movingObject;
        _movingObject = false;
        if (!wantCaptureKeyboard && Rl.IsKeyDown(KeyboardKey.LeftShift))
        {
            if (CurrentLevel?.Selection.Object is not null)
            {
                bool left = Rl.IsKeyDown(KeyboardKey.Left);
                bool right = Rl.IsKeyDown(KeyboardKey.Right);
                bool up = Rl.IsKeyDown(KeyboardKey.Up);
                bool down = Rl.IsKeyDown(KeyboardKey.Down);
                int dx = left == right ? 0 : left ? -1 : 1;
                int dy = up == down ? 0 : up ? -1 : 1;
                if (dx != 0 || dy != 0)
                {
                    double delta = Rl.GetFrameTime() * 600;
                    bool moved = WmeUtils.MoveObject(CurrentLevel.Selection.Object, delta * dx, delta * dy, CurrentLevel.CommandHistory);
                    if (moved)
                        _movingObject = true;
                }
            }
        }
        // finished moving
        if (wasMovingObject && !_movingObject && CurrentLevel is not null)
        {
            CurrentLevel.CommandHistory.SetAllowMerge(false);
        }

        if (!wantCaptureKeyboard)
        {
            if (Rl.IsKeyPressed(KeyboardKey.F11)) Rl.ToggleFullscreen();
            if (Rl.IsKeyPressed(KeyboardKey.F1)) _showMainMenuBar = !_showMainMenuBar;
            if (CurrentLevel?.Selection.Object is not null && Rl.IsKeyPressed(KeyboardKey.Delete))
            {
                WmeUtils.RemoveObject(CurrentLevel.Selection.Object, CurrentLevel.Level.Desc, CurrentLevel.CommandHistory);
            }
        }

        if (!wantCaptureKeyboard && Rl.IsKeyPressed(KeyboardKey.P))
            ExportWorldImage();
    }

    public Vector2 ScreenToWorld(Vector2 screenPos) =>
        Rl.GetScreenToWorld2D(screenPos - ViewportWindow.Bounds.P1, _cam);

    public void QueueResetCam() => _camResetQueued = true;
    public void ResetCam() => ResetCam(ViewportWindow.Bounds.Width, ViewportWindow.Bounds.Height);

    public void ResetCam(double surfaceW, double surfaceH)
    {
        _cam.Zoom = 1.0f;
        CameraBounds? bounds = CurrentLevel?.Level.Desc.CameraBounds;
        if (bounds is null) return;

        double scale = Math.Min(surfaceW / bounds.W, surfaceH / bounds.H);
        _cam.Offset = new(0);
        _cam.Target = new((float)bounds.X, (float)bounds.Y);
        _cam.Zoom = (float)scale;
    }

    public void ExportWorldImage()
    {
        if (CurrentLevel is null || Canvas is null) return;

        CameraBounds cameraBounds = CurrentLevel.Level.Desc.CameraBounds;
        Image image = GetWorldRect((float)cameraBounds.X, (float)cameraBounds.Y, (int)cameraBounds.W, (int)cameraBounds.H);
        Task.Run(() =>
        {
            string extension = "png";
            Rl.ImageFlipVertical(ref image);
            DialogResult dialogResult = Dialog.FileSave(extension);
            if (dialogResult.IsOk)
            {
                string path = dialogResult.Path;
                path = WmeUtils.ForcePathExtension(path, extension);
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
        CurrentLevel?.Level.DrawOn(Canvas, WmsTransform.IDENTITY, _renderConfig, new RenderContext(), _state);
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
                    Rl.TraceLog(TraceLogLevel.Error, $"Opening level file failed with error: {e.Message}");
                    Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                }
            }
        });
    }

    private void SaveLevelFile()
    {
        if (CurrentLevel is null) return;

        if (CurrentLevel.ReloadMethod is LevelPathLoad lpLoad)
        {
            WmeUtils.SerializeToPath(CurrentLevel.Level, lpLoad.Path);
            TitleBar.SetTitle(lpLoad.Path, false);
            return;
        }

        SaveLevelFileToPath();
    }

    private void SaveLevelFileToPath()
    {
        if (CurrentLevel is null) return;
        Task.Run(() =>
        {
            if (CurrentLevel is null) return;

            string extension = "xml";
            DialogResult result = Dialog.FileSave(extension, Path.GetDirectoryName(PathPrefs.LevelPath));
            if (result.IsOk)
            {
                string path = result.Path;
                path = WmeUtils.ForcePathExtension(path, extension);
                WmeUtils.SerializeToPath(CurrentLevel.Level, path);
                PathPrefs.LevelPath = path;
                CurrentLevel.ReloadMethod = new LevelPathLoad(path);
                TitleBar.SetTitle(path, false);
            }
        });
    }

    public void CloseCurrentLevel()
    {
        if (CurrentLevel is not null)
        {
            DeregisterLevel(CurrentLevel);
            LoadedLevels.Remove(CurrentLevel);
        }
        CurrentLevel = null;
        TitleBar.Reset();
        ResetState();
    }

    public void OnLevelReloaded(EditorLevel level, Level newData, ILoadMethod loadMethod)
    {
        level.ResetState();
        level.Level = newData;
        level.ReloadMethod = loadMethod;

        if (CurrentLevel == level)
        {
            QueueResetCam();
            ResetState();
        }
    }

    public void AddNewLevel(EditorLevel editorLevel, bool takeFocus)
    {
        LoadedLevels.Add(editorLevel);
        RegisterLevel(editorLevel);
        if (takeFocus)
        {
            CurrentLevel = editorLevel;
            QueueResetCam();
            ResetState();
        }
    }

    private void RegisterLevel(EditorLevel editorLevel)
    {
        editorLevel.CommandHistoryChanged += OnCommandHistoryChanged;
    }

    private void DeregisterLevel(EditorLevel editorLevel)
    {
        editorLevel.CommandHistoryChanged -= OnCommandHistoryChanged;
    }

    private void OnCommandHistoryChanged(object? sender, EventArgs args)
    {
        if (TitleBar.OpenLevelFile is not null)
            TitleBar.SetTitle(TitleBar.OpenLevelFile, true);
    }

    private void ReloadMap()
    {
        if (CurrentLevel is null) return;
        Task.Run(() =>
        {
            try
            {
                LevelLoader.ReImport(CurrentLevel);
                if (CurrentLevel.ReloadMethod is LevelPathLoad lpLoad)
                    TitleBar.SetTitle(lpLoad.Path, false);
            }
            catch (Exception e)
            {
                Rl.TraceLog(TraceLogLevel.Error, $"Reloading map failed with error: {e.Message}");
                Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
            }
        });
    }

    public void ResetState()
    {
        _movingObject = false;
        _renderConfig.Time = TimeSpan.FromTicks(0);
        CurrentLevel?.ResetState();
        Canvas?.ClearTextureCache();
        ResetRenderState();
    }

    ~Editor()
    {
        rlImGui.Shutdown();
        Rl.CloseWindow();
    }
}