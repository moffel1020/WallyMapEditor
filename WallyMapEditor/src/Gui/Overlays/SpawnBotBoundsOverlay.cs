using WallyMapSpinzor2;

namespace WallyMapEditor;

public class SpawnBotBoundsOverlay(SpawnBotBounds bounds) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(bounds.X, bounds.Y, bounds.W, bounds.H);

    public void Draw(OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorSpawnBotBoundsBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorSpawnBotBoundsBox;

        ResizableBox.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        ResizableBox.CircleRadius = data.OverlayConfig.RadiusSpawnBotBoundsCorner;

        ResizableBox.Update(data, bounds.X, bounds.Y, bounds.W, bounds.H);
        (double x, double y, double w, double h) = ResizableBox.Bounds;

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<(double, double, double, double)>(
                val => (bounds.X, bounds.Y, bounds.W, bounds.H) = val,
                (bounds.X, bounds.Y, bounds.W, bounds.H),
                (x, y, w, h)));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (bounds.X, bounds.Y) = val,
                (bounds.X, bounds.Y),
                (x, y)));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}