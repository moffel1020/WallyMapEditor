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

    public delegate void ResetCamPossibleEventHandler(ViewportWindow? sender);
    public event ResetCamPossibleEventHandler? ResetCamPossible;

    public void Show(IEnumerable<EditorLevel> loadedLevels, ref EditorLevel? currentLevel, bool cameraResetQueued = false)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        ImGui.Begin("Viewport", ref _open, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        if (ImGui.BeginTabBar("levels", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
        {
            foreach (EditorLevel l in loadedLevels)
            {
                if (ImGui.BeginTabItem($"{l.Level.Desc.LevelName}###{l.GetHashCode()}"))
                {
                    currentLevel = l;

                    ImGui.BeginChild("");

                    Focussed = ImGui.IsWindowFocused();
                    Hovered = ImGui.IsWindowHovered();
                    Bounds.P1 = ImGui.GetCursorScreenPos();
                    Bounds.P2 = ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail();
                    if (SizeChanged()) CreateFramebuffer((int)Bounds.Size.X, (int)Bounds.Size.Y);
                    if (cameraResetQueued) ResetCamPossible?.Invoke(this);

                    rlImGui.ImageRenderTexture(Framebuffer);

                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar();
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