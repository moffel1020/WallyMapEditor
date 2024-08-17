using WallyMapSpinzor2;

namespace WallyMapEditor;

public class NavNodeOverlay(NavNode node) : IOverlay
{
    public DragBox Box { get; set; } = new(node.X, node.Y, 0, 0);
    public void Draw(OverlayData data)
    {
        Box.Color = data.OverlayConfig.ColorNavNodeBox;
        Box.UsingColor = data.OverlayConfig.UsingColorNavNodeBox;
        Box.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (node.Parent is not null)
        {
            (double dynOffsetX, double dynOffsetY) = node.Parent.GetOffset(data.Context);
            (offsetX, offsetY) = (node.Parent.X + dynOffsetX, node.Parent.Y + dynOffsetY);
        }

        Box.W = data.RenderConfig.RadiusNavNode * 2 + data.OverlayConfig.SizeOffsetNavNodeBox;
        Box.H = data.RenderConfig.RadiusNavNode * 2 + data.OverlayConfig.SizeOffsetNavNodeBox;
        Box.Middle = (node.X + offsetX, node.Y + offsetY);

        Box.Update(data, true);

        if (Box.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (node.X, node.Y) = val,
                (node.X, node.Y),
                (Box.Middle.Item1 - offsetX, Box.Middle.Item2 - offsetY)));
        }

        return Box.Dragging;
    }
}