using WallyMapSpinzor2;

namespace WallyMapEditor;

public class OverlayData
{
    public required ViewportWindow Viewport { get; init; }
    public required RenderContext Context { get; init; }
    public required RenderConfig RenderConfig { get; init; }
    public required OverlayConfig OverlayConfig { get; init; }
}