namespace WallyMapSpinzor2.Raylib;

public class RespawnOverlay(Respawn res) : IOverlay
{
    public DragBox Box { get; set; } = new(res.X, res.Y, 0, 0);
    public void Draw(OverlayData data)
    {
        Box.Color = data.OverlayConfig.ColorRespawnBox;
        Box.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (res.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(res.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (res.Parent.X + dynOffset.Item1, res.Parent.Y + dynOffset.Item2);

        Box.W = data.RenderConfig.RadiusRespawn * 2 + data.OverlayConfig.SizeOffsetRespawnBox;
        Box.H = data.RenderConfig.RadiusRespawn * 2 + data.OverlayConfig.SizeOffsetRespawnBox;
        Box.Middle = (res.X + offsetX, res.Y + offsetY);

        Box.Update(data, true);

        if (Box.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (res.X, res.Y) = val,
                (res.X, res.Y),
                (Box.Middle.Item1 - offsetX, Box.Middle.Item2 - offsetY)));
        }

        return Box.Dragging;
    }
}