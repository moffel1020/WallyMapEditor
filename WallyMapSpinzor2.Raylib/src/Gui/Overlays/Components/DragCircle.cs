using System.Numerics;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class DragCircle(double x, double y)
{
    public const int RADIUS = 30;

    public static Raylib_cs.Color Color => Raylib_cs.Color.Gray with { A = 190 };
    public static Raylib_cs.Color UsingColor => Raylib_cs.Color.White with { A = 190 };

    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public bool Hovered { get; private set; } = false;
    public bool Dragging { get; private set; } = false;

    public Vector2 Coords => new((float)X, (float)Y);

    public void Draw(OverlayData data)
    {
        if (Hovered || Dragging)
            Rl.DrawCircleV(Coords, RADIUS, UsingColor);
        else
            Rl.DrawCircleV(Coords, RADIUS, Color);
    }

    public void Update(OverlayData data, bool allowDragging)
    {
        Vector2 worldPos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        Hovered = data.Viewport.Hovered && Rl.CheckCollisionPointCircle(worldPos, Coords, RADIUS);

        if (!allowDragging) Dragging = false;

        if (allowDragging && Hovered && Rl.IsMouseButtonPressed(MouseButton.Left))
            Dragging = true;

        if (allowDragging && Dragging && Rl.IsMouseButtonReleased(MouseButton.Left))
            Dragging = false;

        if (Dragging) (X, Y) = (worldPos.X, worldPos.Y);
    }
}
