using WallyMapSpinzor2;
using Raylib_cs;

namespace WallyMapEditor;

public class OverlayData
{
    public required ViewportWindow Viewport { get; init; }
    public required Camera2D Cam { get; init; }
    public required RenderContext Context { get; init; }
    public required RenderConfig RenderConfig { get; init; }
    public required OverlayConfig OverlayConfig { get; init; }
    // TODO: This shouldn't be needed - OverlayManager has a reference to the EditorLevel
    public required Level? Level { get; init; }
}