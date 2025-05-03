using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WallyMapSpinzor2;
using Raylib_cs;

namespace WallyMapEditor;

public class CollisionOverlay(AbstractCollision col) : IOverlay
{
    public DragCircle Circle1 { get; set; } = new(col.X1, col.Y1);
    public DragCircle Circle2 { get; set; } = new(col.X2, col.Y2);
    public DragCircle Center { get; set; } = new((col.X1 + col.X2) / 2, (col.Y1 + col.Y2) / 2);
    public DragCircle Anchor { get; set; } = new(col.AnchorX ?? double.NaN, col.AnchorY ?? double.NaN);

    public RlColor SnapPointColor { get; set; } = RlColor.Green;
    public double SnapPointRadius { get; set; } = 30;

    private bool HasAnchor => !double.IsNaN(Anchor.X) && !double.IsNaN(Anchor.Y);
    private (double, double)? _snapToPoint;
    private (double, double)? _centerDragOrigin;

    private const double MAX_SNAP_DISTANCE = 4000;  // squared distance
    private const double SNAP_POINT_VISIBLE_DISTANCE = 100000; // squared distance

    public const int ROUND_DECIMALS = 6;

    public virtual bool Update(OverlayData data, CommandHistory cmd)
    {
        Circle1.Radius = Circle2.Radius = Center.Radius = data.OverlayConfig.RadiusCollisionPoint;
        Anchor.Radius = data.OverlayConfig.RadiusCollisionAnchor;

        (double offsetX, double offsetY) = (0, 0);
        (double dynOffsetX, double dynOffsetY) = (0, 0);
        if (col.Parent is not null)
        {
            (dynOffsetX, dynOffsetY) = col.Parent.GetOffset(data.Context);
            (offsetX, offsetY) = (dynOffsetX + col.Parent.X, dynOffsetY + col.Parent.Y);
        }

        (Circle1.X, Circle1.Y) = (col.X1 + offsetX, col.Y1 + offsetY);
        (Circle2.X, Circle2.Y) = (col.X2 + offsetX, col.Y2 + offsetY);
        (Center.X, Center.Y) = ((Circle1.X + Circle2.X) / 2, (Circle1.Y + Circle2.Y) / 2);
        (Anchor.X, Anchor.Y) = ((col.AnchorX ?? double.NaN) + dynOffsetX, (col.AnchorY ?? double.NaN) + dynOffsetY);

        Circle1.Update(data, true);
        Circle2.Update(data, !Circle1.Dragging);

        if (HasAnchor) Anchor.Update(data, !Circle1.Dragging && !Circle2.Dragging);

        _centerDragOrigin ??= (Center.X, Center.Y);
        Center.Update(data, !Circle1.Dragging && !Circle2.Dragging && !Anchor.Dragging);
        if (!Center.Dragging) _centerDragOrigin = null;

        if (Circle1.Dragging)
        {
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Circle1, Circle2);

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
                _snapToPoint = SnapDrag(col, Circle1, data);

            double newX = Math.Round(Circle1.X - offsetX, ROUND_DECIMALS);
            double newY = Math.Round(Circle1.Y - offsetY, ROUND_DECIMALS);
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (col.X1, col.Y1) = (val1, val2),
                col.X1, col.Y1,
                newX, newY
            ));
        }

        if (Circle2.Dragging)
        {
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Circle2, Circle1);

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
                _snapToPoint = SnapDrag(col, Circle2, data);

            double newX = Math.Round(Circle2.X - offsetX, ROUND_DECIMALS);
            double newY = Math.Round(Circle2.Y - offsetY, ROUND_DECIMALS);
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (col.X2, col.Y2) = (val1, val2),
                col.X2, col.Y2,
                newX, newY
            ));
        }

        if (HasAnchor && Anchor.Dragging)
        {
            cmd.Add(new PropChangeCommand<double?, double?>(
                (val1, val2) => (col.AnchorX, col.AnchorY) = (val1, val2),
                col.AnchorX, col.AnchorY,
                Anchor.X - dynOffsetX, Anchor.Y - dynOffsetY
            ));
        }

        if (Center.Dragging)
        {
            // _centerDragOrigin shouldn't be null here, but check anyways
            if (_centerDragOrigin is not null && Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Center, _centerDragOrigin.Value.Item1, _centerDragOrigin.Value.Item2);

            double centerX = Math.Round(Center.X - offsetX, ROUND_DECIMALS);
            double centerY = Math.Round(Center.Y - offsetY, ROUND_DECIMALS);

            double diffX = col.X2 - col.X1;
            double diffY = col.Y2 - col.Y1;
            double newX1 = centerX - diffX / 2;
            double newY1 = centerY - diffY / 2;
            double newX2 = centerX + diffX / 2;
            double newY2 = centerY + diffY / 2;

            cmd.Add(new PropChangeCommand<double, double, double, double>(
                (val1, val2, val3, val4) => (col.X1, col.Y1, col.X2, col.Y2) = (val1, val2, val3, val4),
                col.X1, col.Y1, col.X2, col.Y2,
                newX1, newY1, newX2, newY2
            ));
        }

        return Circle1.Dragging || Circle2.Dragging || Center.Dragging || Anchor.Dragging;
    }

    public virtual void Draw(OverlayData data)
    {
        Circle1.Color = Circle2.Color = Center.Color = data.OverlayConfig.ColorCollisionPoint;
        Circle1.UsingColor = Circle2.UsingColor = Center.UsingColor = data.OverlayConfig.UsingColorCollisionPoint;
        Anchor.Color = data.OverlayConfig.ColorCollisionAnchor;
        Anchor.UsingColor = data.OverlayConfig.UsingColorCollisionAnchor;
        SnapPointRadius = data.OverlayConfig.RadiusCollisionSnapPoint;
        SnapPointColor = data.OverlayConfig.ColorCollisionSnapPoint;

        Circle1.Draw(data);
        Circle2.Draw(data);
        if (HasAnchor) Anchor.Draw(data);
        Center.Draw(data);

        if (_snapToPoint is not null)
        {
            (double px, double py) = _snapToPoint.Value;
            if ((Circle1.Dragging && DistanceSquared(px, py, Circle1.X, Circle1.Y) < SNAP_POINT_VISIBLE_DISTANCE)
                || (Circle2.Dragging && DistanceSquared(px, py, Circle2.X, Circle2.Y) < SNAP_POINT_VISIBLE_DISTANCE))
            {
                Rl.DrawCircleV(new Vector2((float)px, (float)py), (float)SnapPointRadius, SnapPointColor);
            }
        }
    }

    private static void LockAxisDrag(DragCircle dragging, DragCircle other)
    {
        LockAxisDrag(dragging, other.X, other.Y);
    }

    private static void LockAxisDrag(DragCircle dragging, double x, double y)
    {
        (double newX, double newY) = (dragging.X, dragging.Y);
        if (Math.Abs(dragging.X - x) < Math.Abs(dragging.Y - y))
            newX = x;
        else
            newY = y;

        (dragging.X, dragging.Y) = (newX, newY);
    }

    private static (double, double)? SnapDrag(AbstractCollision current, DragCircle dragging, OverlayData data)
    {
        if (data.Level is null) return null;

        (double, double)? closest = data.Level.Desc.Collisions.Where(c => c != current)
            .SelectMany(IEnumerable<(double, double)> (c) => [(c.X1, c.Y1), (c.X2, c.Y2)])
            .Concat(CollisionPointsAbsolute(data.Level.Desc.DynamicCollisions, data.Context, current))
            .OrderBy(p => DistanceSquared(dragging.X, dragging.Y, p.Item1, p.Item2))
            .FirstOrDefault();

        if (closest is not null && DistanceSquared(dragging.X, dragging.Y, closest.Value.Item1, closest.Value.Item2) <= MAX_SNAP_DISTANCE)
            (dragging.X, dragging.Y) = (closest.Value.Item1, closest.Value.Item2);

        return closest;
    }

    private static IEnumerable<(double, double)> CollisionPointsAbsolute(IEnumerable<DynamicCollision> dynamics, RenderContext context, AbstractCollision exclude)
    {
        foreach (DynamicCollision dc in dynamics)
        {
            if (context.PlatIDDynamicOffset.TryGetValue(dc.PlatID, out (double, double) dynOffset))
            {
                (double offX, double offY) = dynOffset;
                foreach (AbstractCollision ac in dc.Children)
                {
                    if (ac == exclude) continue;
                    yield return (ac.X1 + dc.X + offX, ac.Y1 + dc.Y + offY);
                    yield return (ac.X2 + dc.X + offX, ac.Y2 + dc.Y + offY);
                }
            }
        }
    }

    private static double DistanceSquared(double x1, double y1, double x2, double y2) => (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
}