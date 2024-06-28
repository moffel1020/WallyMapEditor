namespace WallyMapSpinzor2.Raylib;

public class CollisionOverlay(AbstractCollision col) : IOverlay
{
    public DragCircle Circle1 { get; set; } = new(col.X1, col.Y1);
    public DragCircle Circle2 { get; set; } = new(col.X2, col.Y2);
    public DragCircle? Anchor { get; set; } =
    col.AnchorX is not null && col.AnchorY is not null
        ? new(col.AnchorX.Value, col.AnchorY.Value)
        : null;

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (col.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(col.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (col.Parent.X + dynOffset.Item1, col.Parent.Y + dynOffset.Item2);

        (Circle1.X, Circle1.Y) = (col.X1 + offsetX, col.Y1 + offsetY);
        (Circle2.X, Circle2.Y) = (col.X2 + offsetX, col.Y2 + offsetY);
        if (Anchor is null)
        {
            if (col.AnchorX is not null && col.AnchorY is not null)
                Anchor = new(col.AnchorX.Value, col.AnchorY.Value);
        }
        else
        {
            if (col.AnchorX is null || col.AnchorY is null)
                Anchor = null;
            else
                (Anchor.X, Anchor.Y) = (col.AnchorX.Value, col.AnchorY.Value);
        }

        Circle1.Update(data, true);
        Circle2.Update(data, !Circle1.Dragging);
        Anchor?.Update(data, !Circle1.Dragging && !Circle2.Dragging);

        if (Circle1.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X1, col.Y1) = val,
                (col.X1, col.Y1),
                (Circle1.X - offsetX, Circle1.Y - offsetY)));
        }

        if (Circle2.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X2, col.Y2) = val,
                (col.X2, col.Y2),
                (Circle2.X - offsetX, Circle2.Y - offsetY)));
        }

        if (Anchor is not null && Anchor.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.AnchorX, col.AnchorY) = val,
                (col.AnchorX!.Value, col.AnchorY!.Value),
                (Anchor.X - offsetX, Anchor.Y - offsetY)));
        }

        return
            Circle1.Dragging || Circle1.Hovered ||
            Circle2.Dragging || Circle2.Hovered ||
            (Anchor is not null && (Anchor.Dragging || Anchor.Hovered));
    }

    public void Draw(OverlayData data)
    {
        Circle1.Draw(data);
        Circle2.Draw(data);
        Anchor?.Draw(data);
    }
}