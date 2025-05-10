using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

namespace WallyMapEditor;

public class ViewportWindow
{
    public RenderTexture2D Framebuffer { get; set; }
    public ViewportBounds Bounds { get; set; } = new();
    public bool Focussed { get; private set; }
    public bool Hovered { get; private set; }
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    public delegate void ClosedLevelEventHandler(ViewportWindow? sender, EditorLevel level);
    public event ClosedLevelEventHandler? ClosedLevel;

    private EditorLevel? _currentLevel;

    public void Show(IEnumerable<EditorLevel> loadedLevels, ref EditorLevel? currentLevel)
    {
        bool needSetSelected = _currentLevel != currentLevel;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.Begin("Viewport", ref _open, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        Focussed = ImGui.IsWindowFocused();
        Hovered = ImGui.IsWindowHovered();

        if (ImGui.BeginTabBar("levels", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
        {
            foreach (EditorLevel l in loadedLevels)
            {
                bool open = true;
                bool setSelected = needSetSelected && currentLevel == l;

                string title = l.LevelTitle;
                bool saved = l.IsSaved;
                string? tooltip = l.LevelTooltip;

                ImGuiTabItemFlags selectFlags = setSelected ? ImGuiTabItemFlags.SetSelected : 0;
                ImGuiTabItemFlags unsavedFlags = saved ? 0 : ImGuiTabItemFlags.UnsavedDocument;
                ImGuiTabItemFlags flags = selectFlags | unsavedFlags;
                bool beginTabItem = ImGui.BeginTabItem($"{title}###{l.GetHashCode()}", ref open, flags);
                if (tooltip is not null && ImGui.IsItemHovered())
                    ImGui.SetTooltip(tooltip);
                if (beginTabItem)
                {
                    currentLevel = l;

                    Bounds.P1 = ImGui.GetCursorScreenPos();
                    Bounds.P2 = ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail();
                    if (SizeChanged()) CreateFramebuffer((int)Bounds.Size.X, (int)Bounds.Size.Y);

                    rlImGui.ImageRenderTexture(Framebuffer);

                    if (!l.DidCameraInit)
                        l.ResetCam(Bounds.Width, Bounds.Height);

                    ImGui.EndTabItem();
                }
                if (!open)
                    ClosedLevel?.Invoke(this, l);
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
        ImGui.PopStyleVar();

        _currentLevel = currentLevel;
    }

    public Vector2 ScreenToWorld(Vector2 screenPos, Camera2D cam) =>
        Rl.GetScreenToWorld2D(screenPos - Bounds.P1, cam);

    private void CreateFramebuffer(int width, int height)
    {
        if (Framebuffer.Id != 0) Rl.UnloadRenderTexture(Framebuffer);
        Framebuffer = Rl.LoadRenderTexture(width, height);
    }

    private bool SizeChanged() => Framebuffer.Texture.Width != Bounds.Size.X || Framebuffer.Texture.Height != Bounds.Size.Y;

    ~ViewportWindow()
    {
        Rl.UnloadRenderTexture(Framebuffer);
    }
}