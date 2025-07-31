using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class SpawnBotBoundsOverlay(SpawnBotBounds bounds) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(bounds.X, bounds.Y, bounds.W, bounds.H);

    public void Draw(EditorLevel level, OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorSpawnBotBoundsBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorSpawnBotBoundsBox;

        ResizableBox.Draw(data);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        CommandHistory cmd = level.CommandHistory;

        ResizableBox.CircleRadius = data.OverlayConfig.RadiusSpawnBotBoundsCorner;

        ResizableBox.Update(level.Camera, data, bounds.X, bounds.Y, bounds.W, bounds.H);
        (double x, double y, double w, double h) = ResizableBox.Bounds;

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<double, double, double, double>(
                (val1, val2, val3, val4) => (bounds.X, bounds.Y, bounds.W, bounds.H) = (val1, val2, val3, val4),
                bounds.X, bounds.Y, bounds.W, bounds.H,
                x, y, w, h
            ));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (bounds.X, bounds.Y) = (val1, val2),
                bounds.X, bounds.Y,
                x, y
            ));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}