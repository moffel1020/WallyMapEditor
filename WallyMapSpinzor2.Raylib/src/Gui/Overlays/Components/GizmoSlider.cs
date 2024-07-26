using System;
using System.Numerics;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class GizmoSlider(double x, double y)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public float Rotation { get; set; } = 0;

    public double Sensitivity { get; set; } = 0.1;
    public double LineWidth { get; set; } = 20;
    public double Length { get; set; } = 250;
    public RlColor Color { get; set; } = RlColor.Yellow;
    public RlColor UsingColor { get; set; } = RlColor.Orange;

    public Vector2 Coords => new((float)X, (float)Y);

    public double Value { get; private set; }

    public bool Hovered { get; private set; }
    public bool Dragging { get; private set; }

    private double? _lastMouseOffset;

#pragma warning disable IDE0060
    public void Draw(OverlayData data)
#pragma warning restore IDE0060
    {
        Vector2 rotOrigin = new(0, (float)LineWidth / 2);
        Rectangle rect = new((float)X, (float)Y, (float)Length, (float)LineWidth);
        if (Hovered || Dragging)
            Rl.DrawRectanglePro(rect, rotOrigin, Rotation, UsingColor);
        else
            Rl.DrawRectanglePro(rect, rotOrigin, Rotation, Color);
    }

    public void Update(OverlayData data, double currentValue, bool allowDragging)
    {
        Rectangle rect = new((float)X, (float)Y, (float)Length, (float)LineWidth);

        Vector2 rotOrigin = new(0, (float)LineWidth / 2);
        Vector2 mousePos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);

        Hovered = Wms2RlUtils.CheckCollisionPointRotatedRec(mousePos, rect, Rotation * Math.PI / 180, rotOrigin);
        Value = currentValue;

        if (!allowDragging)
        {
            _lastMouseOffset = null;
            Dragging = false;
        }

        if (allowDragging && Dragging && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _lastMouseOffset = null;
            Dragging = false;
        }

        if (allowDragging && Hovered && Rl.IsMouseButtonPressed(MouseButton.Left))
        {
            _lastMouseOffset = Vector2.Distance(mousePos, new((float)X, (float)Y));
            Dragging = true;
        }

        if (Dragging && _lastMouseOffset is not null)
        {
            Vector2 gizmoVec = new((float)Math.Cos(Rotation * Math.PI / 180), (float)Math.Sin(Rotation * Math.PI / 180));
            Vector2 oldPos = (float)_lastMouseOffset.Value * gizmoVec;

            Vector2 proj = Vector2.Dot(mousePos - new Vector2((float)X, (float)Y), gizmoVec) / Vector2.Dot(gizmoVec, gizmoVec) * gizmoVec; // orthogonal projection onto gizmo line
            Vector2 diff = proj - oldPos;

            int side = gizmoVec.X * proj.X < 0 || gizmoVec.Y * proj.Y < 0 ? -1 : 1;
            int dragDir = proj.Length() < oldPos.Length() ? -1 : 1;

            Value += side * dragDir * diff.Length() * Sensitivity * Rl.GetFrameTime();

            _lastMouseOffset = side * proj.Length();
        }
    }
}