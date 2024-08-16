using WallyMapSpinzor2;

namespace WallyMapEditor;

public class ItemSpawnOverlay(AbstractItemSpawn item) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(item.X, item.Y, item.W, item.H);

    public void Draw(OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorItemSpawnBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorItemSpawnBox;

        ResizableBox.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        ResizableBox.CircleRadius = data.OverlayConfig.RadiusItemSpawnCorner;

        (double offsetX, double offsetY) = (0, 0);
        if (item.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(item.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (item.Parent.X + dynOffset.Item1, item.Parent.Y + dynOffset.Item2);

        ResizableBox.Update(data, item.X + offsetX, item.Y + offsetY, item.W, item.H);
        (double x, double y, double w, double h) = ResizableBox.Bounds;

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<(double, double, double, double)>(
                val => (item.X, item.Y, item.W, item.H) = val,
                (item.X, item.Y, item.W, item.H),
                (x - offsetX, y - offsetY, w, h)));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (item.X, item.Y) = val,
                (item.X, item.Y),
                (x - offsetX, y - offsetY)));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}