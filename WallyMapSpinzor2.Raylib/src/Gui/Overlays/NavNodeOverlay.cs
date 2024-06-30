namespace WallyMapSpinzor2.Raylib;

public class NavNodeOverlay(NavNode node) : IOverlay
{
    public const int SIZE_OFFSET = 40;

    public DragBox Box { get; set; } = new(node.X, node.Y, 0, 0);
    public void Draw(OverlayData data)
    {
        Box.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (node.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(node.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (node.Parent.X + dynOffset.Item1, node.Parent.Y + dynOffset.Item2);

        Box.W = data.Config.RadiusRespawn * 2 + SIZE_OFFSET;
        Box.H = data.Config.RadiusRespawn * 2 + SIZE_OFFSET;
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