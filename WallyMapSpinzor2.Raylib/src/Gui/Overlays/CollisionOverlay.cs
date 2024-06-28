namespace WallyMapSpinzor2.Raylib;

public class CollisionOverlay(AbstractCollision col) : IOverlay
{
    public DragCircle Circle1 { get; set; } = new(col.X1, col.Y1);
    public DragCircle Circle2 { get; set; } = new(col.X2, col.Y2);

    public DragCircle Anchor { get; set; } = new(col.AnchorX ?? double.NaN, col.AnchorY ?? double.NaN)
    {
        Color = Raylib_cs.Color.DarkGreen with { A = 190 },
        UsingColor = Raylib_cs.Color.Green with { A = 190 },
    };

    private bool HasAnchor => !double.IsNaN(Anchor.X) && !double.IsNaN(Anchor.Y);

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        (double dynOffsetX, double dynOffsetY) = (0, 0);
        if (col.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(col.Parent.PlatID, out (double, double) dynOffset))
        {
            (offsetX, offsetY) = (col.Parent.X + dynOffset.Item1, col.Parent.Y + dynOffset.Item2);
            (dynOffsetX, dynOffsetY) = dynOffset;
        }

        (Circle1.X, Circle1.Y) = (col.X1 + offsetX, col.Y1 + offsetY);
        (Circle2.X, Circle2.Y) = (col.X2 + offsetX, col.Y2 + offsetY);
        (Anchor.X, Anchor.Y) = ((col.AnchorX ?? double.NaN) + dynOffsetX, (col.AnchorY ?? double.NaN) + dynOffsetY);

        Circle1.Update(data, true);
        Circle2.Update(data, !Circle1.Dragging);
        if (HasAnchor) Anchor.Update(data, !Circle1.Dragging && !Circle2.Dragging);

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

        if (HasAnchor && Anchor.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double?, double?)>(
                val => (col.AnchorX, col.AnchorY) = val,
                (col.AnchorX, col.AnchorY),
                (Anchor.X - dynOffsetX, Anchor.Y - dynOffsetY)));
        }

        return
            Circle1.Dragging || Circle1.Hovered ||
            Circle2.Dragging || Circle2.Hovered ||
            Anchor.Dragging || Anchor.Hovered;
    }

    public void Draw(OverlayData data)
    {
        Circle1.Draw(data);
        Circle2.Draw(data);
        if (HasAnchor) Anchor.Draw(data);
    }
}