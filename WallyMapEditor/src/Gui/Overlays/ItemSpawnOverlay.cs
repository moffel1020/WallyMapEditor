using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class ItemSpawnOverlay(AbstractItemSpawn item) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(item.X, item.Y, item.W, item.H);

    public void Draw(EditorLevel level, OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorItemSpawnBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorItemSpawnBox;

        ResizableBox.Draw(data);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        CommandHistory cmd = level.CommandHistory;

        ResizableBox.CircleRadius = data.OverlayConfig.RadiusItemSpawnCorner;

        (double offsetX, double offsetY) = (0, 0);
        if (item.Parent is not null)
        {
            (double dynOffsetX, double dynOffsetY) = item.Parent.GetOffset(data.Context);
            (offsetX, offsetY) = (dynOffsetX + item.Parent.X, dynOffsetY + item.Parent.Y);
        }

        ResizableBox.Update(level.Camera, data, item.X + offsetX, item.Y + offsetY, item.W, item.H);
        (double x, double y, double w, double h) = ResizableBox.Bounds;

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<double, double, double, double>(
                (val1, val2, val3, val4) => (item.X, item.Y, item.W, item.H) = (val1, val2, val3, val4),
                item.X, item.Y, item.W, item.H,
                x - offsetX, y - offsetY, w, h
            ));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (item.X, item.Y) = (val1, val2),
                item.X, item.Y,
                x - offsetX, y - offsetY
            ));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}