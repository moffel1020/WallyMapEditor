namespace WallyMapEditor;
using WallyMapSpinzor2;

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
        if (res.Parent is not null)
        {
            (double dynOffsetX, double dynOffsetY) = res.Parent.GetOffset(data.Context);
            (offsetX, offsetY) = (dynOffsetX + res.Parent.X, dynOffsetY + res.Parent.Y);
        }

        Box.W = data.RenderConfig.RadiusRespawn * 2 + data.OverlayConfig.SizeOffsetRespawnBox;
        Box.H = data.RenderConfig.RadiusRespawn * 2 + data.OverlayConfig.SizeOffsetRespawnBox;
        Box.Middle = (res.X + offsetX, res.Y + offsetY);

        Box.Update(data, true);

        if (Box.Dragging)
        {
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (res.X, res.Y) = (val1, val2),
                res.X, res.Y,
                Box.Middle.Item1 - offsetX, Box.Middle.Item2 - offsetY
            ));
        }

        return Box.Dragging;
    }
}