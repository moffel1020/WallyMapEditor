namespace WallyMapSpinzor2.Raylib;

public class SpawnBotBoundsOverlay(SpawnBotBounds bounds) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(bounds.X, bounds.Y) { Radius = 100 };
    public DragCircle TopRight { get; set; } = new(bounds.X + bounds.W, bounds.Y) { Radius = 100 };
    public DragCircle BotLeft { get; set; } = new(bounds.X, bounds.Y + bounds.H) { Radius = 100 };
    public DragCircle BotRight { get; set; } = new(bounds.X + bounds.W, bounds.Y + bounds.H) { Radius = 100 };

    public DragBox MoveRect { get; set; } = new(bounds.X, bounds.Y, bounds.W, bounds.H);

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
        (TopLeft.X, TopLeft.Y) = (bounds.X, bounds.Y);
        (TopRight.X, TopRight.Y) = (bounds.X + bounds.W, bounds.Y);
        (BotLeft.X, BotLeft.Y) = (bounds.X, bounds.Y + bounds.H);
        (BotRight.X, BotRight.Y) = (bounds.X + bounds.W, bounds.Y + bounds.H);
        (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = (bounds.X, bounds.Y, bounds.W, bounds.H);

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
                val => (bounds.X, bounds.Y, bounds.W, bounds.H) = val,
                (bounds.X, bounds.Y, bounds.W, bounds.H),
                (TopLeft.X, TopLeft.Y, TopRight.X - TopLeft.X, BotLeft.Y - TopLeft.Y)));
        }

        MoveRect.Update(data, !dragging);

        if (MoveRect.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (bounds.X, bounds.Y) = val,
                (bounds.X, bounds.Y),
                (MoveRect.X, MoveRect.Y)));
        }

        return dragging || MoveRect.Dragging;
    }
}