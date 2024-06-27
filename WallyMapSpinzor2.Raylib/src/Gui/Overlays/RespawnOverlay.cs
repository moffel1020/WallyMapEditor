namespace WallyMapSpinzor2.Raylib;

public class RespawnOverlay(Respawn res) : IOverlay
{
    public const int SIZE_OFFSET = 40;

    public DragBox Box { get; set; } = new(res.X, res.Y, 0, 0);
    public void Draw(OverlayData data)
    {
        Box.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (res.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(res.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (res.Parent.X + dynOffset.Item1, res.Parent.Y + dynOffset.Item2);

        Box.W = data.Config.RadiusRespawn * 2 + SIZE_OFFSET;
        Box.H = data.Config.RadiusRespawn * 2 + SIZE_OFFSET;
        Box.Middle = (res.X + offsetX, res.Y + offsetY);

        Box.Update(data, true);

        if (Box.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (res.X, res.Y) = val,
                (res.X, res.Y),
                (Box.Middle.Item1 - offsetX, Box.Middle.Item2 - offsetY)));
        }

        return Box.Dragging || Box.Hovered || Box.Dragging || Box.Hovered;
    }
}