using System.Collections.Generic;

namespace WallyMapEditor;

// TODO: this should somehow be unified with the AssetOverlay stuff
public class ResizableDragBox(double x, double y, double w, double h)
{
    public DragCircle TopLeft { get; } = new(x, y);
    public DragCircle TopRight { get; } = new(x + w, y);
    public DragCircle BotLeft { get; } = new(y, y + h);
    public DragCircle BotRight { get; } = new(x + w, y + h);
    public DragCircle LeftEdge { get; } = new(x, y + h / 2);
    public DragCircle RightEdge { get; } = new(x + w, y + h / 2);
    public DragCircle TopEdge { get; } = new(x + w / 2, y);
    public DragCircle BottomEdge { get; } = new(x + w / 2, y + h);

    public DragBox MoveRect { get; } = new(x, y, w, h);

    public bool Resizing { get; private set; }
    public bool Moving { get; private set; }

    private IEnumerable<DragCircle> GetCircles()
    {
        yield return TopLeft;
        yield return TopRight;
        yield return BotLeft;
        yield return BotRight;
        yield return LeftEdge;
        yield return RightEdge;
        yield return TopEdge;
        yield return BottomEdge;
    }

    public float CircleRadius
    {
        get => TopLeft.Radius;
        set { foreach (DragCircle circle in GetCircles()) circle.Radius = value; }
    }

    public RlColor Color
    {
        get => TopLeft.Color;
        set { foreach (DragCircle circle in GetCircles()) circle.Color = value; }
    }

    public RlColor UsingColor
    {
        get => TopLeft.UsingColor;
        set { foreach (DragCircle circle in GetCircles()) circle.UsingColor = value; }
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
            (LeftEdge.X, LeftEdge.Y) = (x, y + h / 2);
            (RightEdge.X, RightEdge.Y) = (x + w, y + h / 2);
            (TopEdge.X, TopEdge.Y) = (x + w / 2, y);
            (BottomEdge.X, BottomEdge.Y) = (x + w / 2, y + h);
            (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = (x, y, w, h);
        }
    }

    public void Draw(OverlayData data)
    {
        foreach (DragCircle circle in GetCircles()) circle.Draw(data);
        MoveRect.Draw(data);
    }

    public void Update(OverlayData data, double x, double y, double w, double h)
    {
        Bounds = (x, y, w, h);

        Moving = false;
        Resizing = false;

        foreach (DragCircle circle in GetCircles())
        {
            circle.Update(data, !Resizing);
            Resizing |= circle.Dragging;
        }

        if (TopLeft.Dragging)
        {
            LeftEdge.X = BotLeft.X = TopLeft.X;
            TopEdge.Y = TopRight.Y = TopLeft.Y;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (TopRight.Dragging)
        {
            RightEdge.X = BotRight.X = TopRight.X;
            TopEdge.Y = TopLeft.Y = TopRight.Y;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (BotLeft.Dragging)
        {
            LeftEdge.X = TopLeft.X = BotLeft.X;
            BottomEdge.Y = BotRight.Y = BotLeft.Y;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (BotRight.Dragging)
        {
            RightEdge.X = TopRight.X = BotRight.X;
            BottomEdge.Y = BotLeft.Y = BotRight.Y;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (LeftEdge.Dragging)
        {
            TopLeft.X = BotLeft.X = LeftEdge.X;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (RightEdge.Dragging)
        {
            TopRight.X = BotRight.X = RightEdge.X;
            TopEdge.X = BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (TopEdge.Dragging)
        {
            TopLeft.Y = TopRight.Y = TopEdge.Y;
            TopEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }
        else if (BottomEdge.Dragging)
        {
            BotLeft.Y = BotRight.Y = BottomEdge.Y;
            BottomEdge.X = (TopRight.X + TopLeft.X) / 2;
            LeftEdge.Y = RightEdge.Y = (BotLeft.Y + TopLeft.Y) / 2;
        }

        // update MoveRect so it displays correctly when drawing
        if (Resizing) (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = Bounds;

        MoveRect.Update(data, !Resizing);
        Moving = MoveRect.Dragging;
        // this will also update the points
        if (Moving) Bounds = (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H);
    }
}
