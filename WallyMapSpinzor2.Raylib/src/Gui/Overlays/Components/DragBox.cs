using System.Numerics;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class DragBox(double x, double y, double w, double h)
{
    public const int LINE_SIZE = 5;

    public Raylib_cs.Color Color { get; set; } = Raylib_cs.Color.Gray with { A = 190 };
    public Raylib_cs.Color UsingColor { get; set; } = Raylib_cs.Color.White with { A = 190 };

    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double W { get; set; } = w;
    public double H { get; set; } = h;
    public Transform Transform { get; set; } = Transform.IDENTITY;

    public bool Hovered { get; private set; }
    public bool Dragging { get; private set; }

    public (double, double) Middle // doesn't account for transform
    {
        get => (X + W / 2, Y + H / 2);
        set => (X, Y) = (value.Item1 - W / 2, value.Item2 - H / 2);
    }

    private (double, double) _mouseDragOffset;

    public void Update(OverlayData data, bool allowDragging)
    {
        Vector2 worldPos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        (double worldX, double worldY) = Transform.CreateInverse(Transform) * (worldPos.X, worldPos.Y);
        (worldPos.X, worldPos.Y) = ((float)worldX, (float)worldY);

        Hovered = data.Viewport.Hovered && Wms2RlUtils.CheckCollisionPointRec(worldPos, new((float)X, (float)Y, (float)W, (float)H));

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
            _mouseDragOffset = (Middle.Item1 - worldX, Middle.Item2 - worldY);
            Dragging = true;
        }

        if (Dragging) Middle = (worldPos.X + _mouseDragOffset.Item1, worldPos.Y + _mouseDragOffset.Item2);
    }

    public void Draw(OverlayData data)
    {
        (double x1, double y1) = Transform * (X, Y);
        (double x2, double y2) = Transform * (X, Y + H);
        (double x3, double y3) = Transform * (X + W, Y);
        (double x4, double y4) = Transform * (X + W, Y + H);

        (float tlX, float tlY) = ((float)x1, (float)y1);
        (float trX, float trY) = ((float)x2, (float)y2);
        (float blX, float blY) = ((float)x3, (float)y3);
        (float brX, float brY) = ((float)x4, (float)y4);

        if (Hovered || Dragging)
        {
            Rl.DrawLineEx(new(tlX, tlY), new(trX, trY), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(tlX, tlY), new(blX, blY), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(trX, trY), new(brX, brY), LINE_SIZE, UsingColor);
            Rl.DrawLineEx(new(blX, blY), new(brX, brY), LINE_SIZE, UsingColor);
        }
        else
        {
            Rl.DrawLineEx(new(tlX, tlY), new(trX, trY), LINE_SIZE, Color);
            Rl.DrawLineEx(new(tlX, tlY), new(blX, blY), LINE_SIZE, Color);
            Rl.DrawLineEx(new(trX, trY), new(brX, brY), LINE_SIZE, Color);
            Rl.DrawLineEx(new(blX, blY), new(brX, brY), LINE_SIZE, Color);
        }
    }
}