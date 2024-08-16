namespace WallyMapEditor;

public class ResizableDragBox(double x, double y, double w, double h)
{
    public DragCircle TopLeft { get; private set; } = new(x, y);
    public DragCircle TopRight { get; private set; } = new(x + w, y);
    public DragCircle BotLeft { get; private set; } = new(y, y + h);
    public DragCircle BotRight { get; private set; } = new(x + w, y + h);

    public DragBox MoveRect { get; private set; } = new(x, y, w, h);

    public bool Resizing { get; private set; }
    public bool Moving { get; private set; }

    public float CircleRadius
    {
        get => TopLeft.Radius;
        set => TopLeft.Radius = TopRight.Radius = BotLeft.Radius = BotRight.Radius = value;
    }

    public RlColor Color
    {
        get => TopLeft.Color;
        set => MoveRect.Color = TopLeft.Color = TopRight.Color = BotLeft.Color = BotRight.Color = value;
    }

    public RlColor UsingColor
    {
        get => TopLeft.UsingColor;
        set => MoveRect.UsingColor = TopLeft.UsingColor = TopRight.UsingColor = BotLeft.UsingColor = BotRight.UsingColor = value;
    }

    public (double, double, double, double) Bounds
    {
        get => (TopLeft.X, TopLeft.Y, TopRight.X - TopLeft.X, BotLeft.Y - TopLeft.Y);
        set
        {
            (double x, double y, double w, double h) = value;
            (TopLeft.X, TopLeft.Y) = (x, y);
            (TopRight.X, TopRight.Y) = (x + w, y);
            (BotLeft.X, BotLeft.Y) = (x, y + h);
            (BotRight.X, BotRight.Y) = (x + w, y + h);
            (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = (x, y, w, h);
        }
    }

    public void Draw(OverlayData data)
    {
        TopLeft.Draw(data);
        TopRight.Draw(data);
        BotLeft.Draw(data);
        BotRight.Draw(data);
        MoveRect.Draw(data);
    }

    public void Update(OverlayData data, double x, double y, double w, double h)
    {
        Bounds = (x, y, w, h);

        Moving = false;
        Resizing = false;

        TopLeft.Update(data, true);
        Resizing = TopLeft.Dragging;
        TopRight.Update(data, !Resizing);
        Resizing |= TopRight.Dragging;
        BotLeft.Update(data, !Resizing);
        Resizing |= BotLeft.Dragging;
        BotRight.Update(data, !Resizing);
        Resizing |= BotRight.Dragging;

        if (TopLeft.Dragging)
        {
            BotLeft.X = TopLeft.X;
            TopRight.Y = TopLeft.Y;
        }
        else if (TopRight.Dragging)
        {
            BotRight.X = TopRight.X;
            TopLeft.Y = TopRight.Y;
        }
        else if (BotLeft.Dragging)
        {
            TopLeft.X = BotLeft.X;
            BotRight.Y = BotLeft.Y;
        }
        else if (BotRight.Dragging)
        {
            TopRight.X = BotRight.X;
            BotLeft.Y = BotRight.Y;
        }

        MoveRect.Update(data, !Resizing);
        Moving = MoveRect.Dragging;
        if (Moving) Bounds = (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H);
    }
}
