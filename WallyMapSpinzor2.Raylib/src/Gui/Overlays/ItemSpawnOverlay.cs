namespace WallyMapSpinzor2.Raylib;

public class ItemSpawnOverlay(AbstractItemSpawn item) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(item.X, item.Y);
    public DragCircle TopRight { get; set; } = new(item.X + item.W, item.Y);
    public DragCircle BotLeft { get; set; } = new(item.X, item.Y + item.H);
    public DragCircle BotRight { get; set; } = new(item.X + item.W, item.Y + item.H);

    public DragBox MoveRect { get; set; } = new(item.X, item.Y, item.W, item.H);

    public void Draw(OverlayData data)
    {
        TopLeft.Draw(data);
        TopRight.Draw(data);
        BotLeft.Draw(data);
        BotRight.Draw(data);
        MoveRect.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (double offsetX, double offsetY) = (0, 0);
        if (item.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(item.Parent.PlatID, out (double, double) dynOffset))
            (offsetX, offsetY) = (item.Parent.X + dynOffset.Item1, item.Parent.Y + dynOffset.Item2);

        (TopLeft.X, TopLeft.Y) = (item.X + offsetX, item.Y + offsetY);
        (TopRight.X, TopRight.Y) = (item.X + offsetX + item.W, item.Y + offsetY);
        (BotLeft.X, BotLeft.Y) = (item.X + offsetX, item.Y + offsetY + item.H);
        (BotRight.X, BotRight.Y) = (item.X + offsetX + item.W, item.Y + offsetY + item.H);
        (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = (item.X + offsetX, item.Y + offsetY, item.W, item.H);

        TopLeft.Update(data, true);
        if (TopLeft.Dragging)
        {
            BotLeft.X = TopLeft.X;
            TopRight.Y = TopLeft.Y;
        }
        bool dragging = TopLeft.Dragging;

        TopRight.Update(data, !dragging);
        if (!dragging && TopRight.Dragging)
        {
            BotRight.X = TopRight.X;
            TopLeft.Y = TopRight.Y;
        }
        dragging |= TopRight.Dragging;

        BotLeft.Update(data, !dragging);
        if (!dragging && BotLeft.Dragging)
        {
            TopLeft.X = BotLeft.X;
            BotRight.Y = BotLeft.Y;
        }
        dragging |= BotLeft.Dragging;

        BotRight.Update(data, !dragging);
        if (!dragging && BotRight.Dragging)
        {
            TopRight.X = BotRight.X;
            BotLeft.Y = BotRight.Y;
        }
        dragging |= BotRight.Dragging;

        if (dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double, double, double)>(
                val => (item.X, item.Y, item.W, item.H) = val,
                (item.X, item.Y, item.W, item.H),
                (TopLeft.X - offsetX, TopLeft.Y - offsetY, TopRight.X - TopLeft.X, BotLeft.Y - TopLeft.Y)));
        }

        MoveRect.Update(data, !dragging);

        if (MoveRect.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (item.X, item.Y) = val,
                (item.X, item.Y),
                (MoveRect.X - offsetX, MoveRect.Y - offsetY)));
        }

        return dragging || TopLeft.Hovered || TopRight.Hovered || BotLeft.Hovered || BotRight.Hovered
            || MoveRect.Dragging || MoveRect.Hovered;
    }
}