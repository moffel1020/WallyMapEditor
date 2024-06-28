using System.Numerics;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class DragBox(double x, double y, double w, double h)
{
    public const int LINE_SIZE = 5;

    public static Raylib_cs.Color Color => Raylib_cs.Color.Gray with { A = 190 };
    public static Raylib_cs.Color UsingColor => Raylib_cs.Color.White with { A = 190 };

    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double W { get; set; } = w;
    public double H { get; set; } = h;

    public bool Hovered { get; private set; }
    public bool Dragging { get; private set; }

    public (double, double) Middle
    {
        get => (X + W / 2, Y + H / 2);
        set => (X, Y) = (value.Item1 - W / 2, value.Item2 - H / 2);
    }

    public Vector2 Coords => new((float)X, (float)Y);

    public void Update(OverlayData data, bool allowDragging)
    {
        Vector2 worldPos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        Hovered = data.Viewport.Hovered && Utils.CheckCollisionPointRec(worldPos, new(Coords, new((float)W, (float)H)));

        if (!allowDragging) Dragging = false;

        if (allowDragging && Hovered && Rl.IsMouseButtonPressed(MouseButton.Left))
            Dragging = true;

        if (allowDragging && Dragging && Rl.IsMouseButtonReleased(MouseButton.Left))
            Dragging = false;

        if (Dragging) Middle = (worldPos.X, worldPos.Y);
    }

    public void Draw(OverlayData data)
    {
        float x = (float)X;
        float y = (float)Y;
        float w = (float)W;
        float h = (float)H;
        // i love float casts
        if (Hovered || Dragging)
        {
            Rl.DrawLineEx(new(x, y), new(x + w, y), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(x + w, y), new(x + w, y + h), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(x + w, y + h), new(x, y + h), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(x, y + h), new(x, y), LINE_SIZE, UsingColor);
        }
        else
        {
            Rl.DrawLineEx(new(x, y), new(x + w, y), LINE_SIZE, Color);
            Rl.DrawLineEx(new(x + w, y), new(x + w, y + h), LINE_SIZE, Color);
            Rl.DrawLineEx(new(x + w, y + h), new(x, y + h), LINE_SIZE, Color);
            Rl.DrawLineEx(new(x, y + h), new(x, y), LINE_SIZE, Color);
        }
    }
}