using System.Collections.Generic;
using Raylib_cs;

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

    public void Update(Camera2D cam, OverlayData data, double x, double y, double w, double h)
    {
        Bounds = (x, y, w, h);

        Moving = false;
        Resizing = false;

        foreach (DragCircle circle in GetCircles())
        {
            circle.Update(cam, data, !Resizing);
            Resizing |= circle.Dragging;
        }

        if (TopLeft.Dragging)
        {
            x = TopLeft.X;
            y = TopLeft.Y;
            w = TopRight.X - TopLeft.X;
            h = BotLeft.Y - TopLeft.Y;
        }
        else if (TopRight.Dragging)
        {
            // x is unchanged
            y = TopRight.Y;
            w = TopRight.X - TopLeft.X;
            h = BotRight.Y - TopRight.Y;
        }
        else if (BotLeft.Dragging)
        {
            x = BotLeft.X;
            // y in unchanged
            w = BotRight.X - BotLeft.X;
            h = BotLeft.Y - TopLeft.Y;
        }
        else if (BotRight.Dragging)
        {
            // x is unchanged
            // y is unchanged
            w = BotRight.X - BotLeft.X;
            h = BotRight.Y - TopRight.Y;
        }
        else if (LeftEdge.Dragging)
        {
            x = LeftEdge.X;
            // y is unchanged
            w = RightEdge.X - LeftEdge.X;
            // h is unchanged
        }
        else if (RightEdge.Dragging)
        {
            // x is unchanged
            // y is unchanged
            w = RightEdge.X - LeftEdge.X;
            // h is unchanged
        }
        else if (TopEdge.Dragging)
        {
            // x is unchanged
            y = TopEdge.Y;
            // w is unchanged
            h = BottomEdge.Y - TopEdge.Y;
        }
        else if (BottomEdge.Dragging)
        {
            // x is unchanged
            // y is unchanged
            // w is unchanged
            h = BottomEdge.Y - TopEdge.Y;
        }

        if (Resizing) Bounds = (x, y, w, h);

        MoveRect.Update(cam, data, !Resizing);
        Moving = MoveRect.Dragging;
        // this will also update the points
        if (Moving) Bounds = (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H);
    }
}
