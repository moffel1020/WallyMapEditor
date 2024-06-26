using System.Numerics;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class DragBox(double x, double y, double size)
{
    public const int LINE_SIZE = 5;

    public static Raylib_cs.Color Color => Raylib_cs.Color.Gray with { A = 190 };
    public static Raylib_cs.Color UsingColor => Raylib_cs.Color.White with { A = 190 };

    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double Size { get; set; } = size;

    public bool Hovered { get; private set; }
    public bool Dragging { get; private set; }

    public (double, double) Middle
    {
        get => (X + Size / 2, Y + Size / 2);
        set => (X, Y) = (value.Item1 - Size / 2, value.Item2 - Size / 2);
    }

    public Vector2 Coords => new((float)X, (float)Y);

    public void Update(OverlayData data, bool allowDragging)
    {
        Vector2 worldPos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        Hovered = data.Viewport.Hovered && Rl.CheckCollisionPointRec(worldPos, new(Coords, new((float)Size)));

        if (!allowDragging) Dragging = false;

        if (allowDragging && Hovered && Rl.IsMouseButtonPressed(MouseButton.Left))
            Dragging = true;

        if (allowDragging && Dragging && Rl.IsMouseButtonReleased(MouseButton.Left))
            Dragging = false;

        if (Dragging) Middle = (worldPos.X, worldPos.Y);
    }

    public void Draw(OverlayData data)
    {
        if (Hovered || Dragging)
            Rl.DrawRectangleLinesEx(new(Coords, (float)Size, (float)Size), LINE_SIZE, UsingColor);
        else
            Rl.DrawRectangleLinesEx(new(Coords, (float)Size, (float)Size), LINE_SIZE, Color);
    }
}