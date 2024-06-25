namespace WallyMapSpinzor2.Raylib;

public class CollisionOverlay(AbstractCollision col) : IOverlay
{
    public DragCircle circle1 = new(col.X1, col.Y1);
    public DragCircle circle2 = new(col.X2, col.Y2);

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);        
        if (col.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(col.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (col.Parent.X + dynOffset.Item1, col.Parent.Y + dynOffset.Item2);

        (circle1.X, circle1.Y) = (col.X1 + offsetX, col.Y1 + offsetY);
        (circle2.X, circle2.Y) = (col.X2 + offsetX, col.Y2 + offsetY);

        circle1.Update(data, true);
        circle2.Update(data, !circle1.Dragging);

        if (circle1.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X1, col.Y1) = val,
                (col.X1, col.Y1),
                (circle1.X - offsetX, circle1.Y - offsetY)));
        }

        if (circle2.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X2, col.Y2) = val,
                (col.X2, col.Y2),
                (circle2.X - offsetX, circle2.Y - offsetY)));
        }

        return circle1.Dragging || circle1.Hovered || circle2.Dragging || circle2.Hovered;
    }

    public void Draw(OverlayData data)
    {
        circle1.Draw(data);
        circle2.Draw(data);
    }
}