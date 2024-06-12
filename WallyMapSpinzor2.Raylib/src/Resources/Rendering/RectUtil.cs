using System;

namespace WallyMapSpinzor2.Raylib;

public readonly record struct RectUtil(double XMin, double YMin, double XMax, double YMax)
{
    public double Width => XMax - XMin;
    public double Height => YMax - YMin;
    public RectUtil ExtendLeft(double amount) => new(XMin - amount, YMin, XMax, YMax);
    public RectUtil ExtendRight(double amount) => new(XMin, YMin, XMax + amount, YMax);
    public RectUtil ExtendTop(double amount) => new(XMin, YMin - amount, XMax, YMax);
    public RectUtil ExtendBottom(double amount) => new(XMin, YMin, XMax, YMax + amount);

    public RectUtil FitWidth(RectUtil other)
    {
        if (Width >= other.Width)
            return this;
        // left side
        if (other.XMax <= XMax)
            return ExtendLeft(other.Width - Width);
        // center
        if (other.XMin <= XMin && XMax <= other.XMax)
            return ExtendLeft(XMin - other.XMin).ExtendRight(other.XMax - XMax);
        // right side
        if (XMin <= other.XMin)
            return ExtendRight(other.Width - Width);
        return this;
    }

    public RectUtil FitHeight(RectUtil other)
    {
        if (Height >= other.Height)
            return this;
        // top side
        if (other.YMax <= YMax)
            return ExtendTop(other.Height - Height);
        // center
        if (other.YMin <= YMin && YMax <= other.YMax)
            return ExtendTop(YMin - other.YMin).ExtendBottom(other.YMax - YMax);
        // bottom side
        if (YMin <= other.YMin)
            return ExtendBottom(other.Height - Height);
        return this;
    }

    public RectUtil FitSize(RectUtil other) => FitWidth(other).FitHeight(other);

    public RectUtil MoveCornerMin(RectUtil other)
    {
        double dXL = other.XMin - XMin;
        double dXR = other.XMax - XMax;
        double dX = (XMin <= other.XMin && other.XMax <= XMax)
            ? 0
            : Math.Abs(dXL) < Math.Abs(dXR)
                ? dXL
                : dXR;
        double dYT = other.YMin - YMin;
        double dYB = other.YMax - YMax;
        double dY = (YMin <= other.YMin && other.YMax <= YMax)
            ? 0
            : Math.Abs(dYT) < Math.Abs(dYB)
                ? dYT
                : dYB;
        return new(XMin + dX, YMin + dY, XMax + dX, YMax + dY);
    }

    public RectUtil UpdateWith(RectUtil other) => FitSize(other).MoveCornerMin(other);
}