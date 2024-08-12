using System;
using System.Numerics;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class RotatePoint(double x, double y)
{
    public const double ANGLE_SNAP = 45 * Math.PI / 180;

    public double Rotation { get; set; } // radians
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public bool Active { get; private set; }

    public RlColor LineColor { get; set; } = RlColor.White;

    private double _mouseRotationOffset;
    private Vector2 Coords => new((float)X, (float)Y);
    private Vector2 _mouseWorldPos = new(0, 0);

    public void Update(OverlayData data, bool allowRotation, double currentRot = 0)
    {
        _mouseWorldPos = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);

        if (!allowRotation)
        {
            Active = false;
            _mouseRotationOffset = 0;
        }

        if (allowRotation && Active && Rl.IsKeyPressed(KeyboardKey.Q))
        {
            _mouseRotationOffset = 0;
            Active = false;
        }
        else if (allowRotation && !Active && Rl.IsKeyPressed(KeyboardKey.Q))
        {
            _mouseRotationOffset = CalculateAngle(Coords, _mouseWorldPos) - currentRot;
            Active = true;
        }

        if (Active)
        {
            Rotation = (CalculateAngle(Coords, _mouseWorldPos) - _mouseRotationOffset) % Math.Tau;

            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                Rotation = ANGLE_SNAP * (int)Math.Round(Rotation / ANGLE_SNAP);
        }
    }

    // Remove unused parameter 'data' if it is not part of a shipped public API [WallyMapSpinzor2.Raylib]
#pragma warning disable IDE0060
    public void Draw(OverlayData data)
#pragma warning restore IDE0060
    {
        if (Active) Rl.DrawLineV(Coords, _mouseWorldPos, LineColor);
    }

    private static double CalculateAngle(Vector2 point1, Vector2 point2) => Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
}