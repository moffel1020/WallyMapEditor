using System.Numerics;

using Raylib_cs;
using Rl = Raylib_cs.Raylib;
using ImGuiNET;
using rlImGui_cs;

namespace WallyMapSpinzor2.Raylib;

public class ViewportWindow : IWindow
{
    public RenderTexture2D Framebuffer { get; set; }
    public ViewportBounds Bounds { get; set; } = new();
    public bool Focussed { get; set; }
    public bool Hovered { get; set; }
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    public void Show()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        ImGui.Begin("Viewport", ref _open, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        Focussed = ImGui.IsWindowFocused();
        Hovered = ImGui.IsWindowHovered();
        Bounds.P1 = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos();
        Bounds.P2 = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();

        if (Framebuffer.Texture.Width != Bounds.Size.X || Framebuffer.Texture.Height != Bounds.Size.Y)
        {
            Rl.UnloadRenderTexture(Framebuffer);
            Framebuffer = Rl.LoadRenderTexture((int)Bounds.Size.X, (int)Bounds.Size.Y);
        }
        rlImGui.ImageRenderTexture(Framebuffer);
        ImGui.End();

        ImGui.PopStyleVar();
    }

    ~ViewportWindow()
    {
        Rl.UnloadRenderTexture(Framebuffer);
    }
}