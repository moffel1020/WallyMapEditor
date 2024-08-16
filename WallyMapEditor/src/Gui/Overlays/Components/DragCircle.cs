using System.Numerics;
using Raylib_cs;

namespace WallyMapEditor;

public class DragCircle(double x, double y)
{
    public float Radius { get; set; } = 30;
    public RlColor Color { get; set; } = RlColor.Gray with { A = 190 };
    public RlColor UsingColor { get; set; } = RlColor.White with { A = 190 };

    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public bool Hovered { get; private set; }
    public bool Dragging { get; private set; }

    public (double, double) Position
    {
        get => (X, Y);
        set => (X, Y) = value;
    }

    private Vector2 Coords => new((float)X, (float)Y);
    private (double, double) _mouseDragOffset;

    // Remove unused parameter 'data' if it is not part of a shipped public API [WallyMapEditor]
#pragma warning disable IDE0060
    public void Draw(OverlayData data)
#pragma warning restore IDE0060
    {
        if (Hovered || Dragging)
            Rl.DrawCircleV(Coords, Radius, UsingColor);
        else
            Rl.DrawCircleV(Coords, Radius, Color);
    }

    public void Update(OverlayData data, bool allowDragging)
    {
        Vector2 mousePos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        Hovered = data.Viewport.Hovered && Rl.CheckCollisionPointCircle(mousePos, Coords, Radius);

        if (!allowDragging)
        {
            _mouseDragOffset = (0, 0);
            Dragging = false;
        }

        if (allowDragging && Dragging && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _mouseDragOffset = (0, 0);
            Dragging = false;
        }

        if (allowDragging && Hovered && Rl.IsMouseButtonPressed(MouseButton.Left))
        {
            _mouseDragOffset = (X - mousePos.X, Y - mousePos.Y);
            Dragging = true;
        }

        if (Dragging) (X, Y) = (mousePos.X + _mouseDragOffset.Item1, mousePos.Y + _mouseDragOffset.Item2);
    }
}
