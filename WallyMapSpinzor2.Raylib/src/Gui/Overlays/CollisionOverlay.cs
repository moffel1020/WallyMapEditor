using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class CollisionOverlay(AbstractCollision col) : IOverlay
{
    public DragCircle Circle1 { get; set; } = new(col.X1, col.Y1);
    public DragCircle Circle2 { get; set; } = new(col.X2, col.Y2);
    public DragCircle Anchor { get; set; } = new(col.AnchorX ?? double.NaN, col.AnchorY ?? double.NaN);

    public RlColor SnapPointColor { get; set; } = RlColor.Green;
    public double SnapPointRadius { get; set; } = 30;

    private bool HasAnchor => !double.IsNaN(Anchor.X) && !double.IsNaN(Anchor.Y);
    private (double, double)? _snapToPoint;

    private const double MAX_SNAP_DISTANCE = 4000;  // squared distance
    private const double SNAP_POINT_VISIBLE_DISTANCE = 100000; // squared distance

    private const int ROUND_DECIMALS = 6;

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        Circle1.Radius = Circle2.Radius = data.OverlayConfig.RadiusCollisionPoint;
        Anchor.Radius = data.OverlayConfig.RadiusCollisionAnchor;

        (double offsetX, double offsetY) = (0, 0);
        (double dynOffsetX, double dynOffsetY) = (0, 0);
        if (col.Parent is not null && data.Context.PlatIDDynamicOffset.TryGetValue(col.Parent.PlatID, out (double, double) dynOffset))
        {
            (offsetX, offsetY) = (col.Parent.X + dynOffset.Item1, col.Parent.Y + dynOffset.Item2);
            (dynOffsetX, dynOffsetY) = dynOffset;
        }

        (Circle1.X, Circle1.Y) = (col.X1 + offsetX, col.Y1 + offsetY);
        (Circle2.X, Circle2.Y) = (col.X2 + offsetX, col.Y2 + offsetY);
        (Anchor.X, Anchor.Y) = ((col.AnchorX ?? double.NaN) + dynOffsetX, (col.AnchorY ?? double.NaN) + dynOffsetY);

        Circle1.Update(data, true);
        Circle2.Update(data, !Circle1.Dragging);
        if (HasAnchor) Anchor.Update(data, !Circle1.Dragging && !Circle2.Dragging);

        if (Circle1.Dragging)
        {
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Circle1, Circle2);

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
                _snapToPoint = SnapDrag(col, Circle1, data);

            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X1, col.Y1) = val,
                (col.X1, col.Y1),
                (Math.Round(Circle1.X - offsetX, ROUND_DECIMALS), Math.Round(Circle1.Y - offsetY, ROUND_DECIMALS))));
        }

        if (Circle2.Dragging)
        {
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Circle2, Circle1);

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
                _snapToPoint = SnapDrag(col, Circle2, data);

            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (col.X2, col.Y2) = val,
                (col.X2, col.Y2),
                (Math.Round(Circle2.X - offsetX, ROUND_DECIMALS), Math.Round(Circle2.Y - offsetY, ROUND_DECIMALS))));
        }

        if (HasAnchor && Anchor.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double?, double?)>(
                val => (col.AnchorX, col.AnchorY) = val,
                (col.AnchorX, col.AnchorY),
                (Anchor.X - dynOffsetX, Anchor.Y - dynOffsetY)));
        }

        return Circle1.Dragging || Circle2.Dragging || Anchor.Dragging;
    }

    public void Draw(OverlayData data)
    {
        Circle1.Color = Circle2.Color = data.OverlayConfig.ColorCollisionPoint;
        Circle1.UsingColor = Circle2.UsingColor = data.OverlayConfig.UsingColorCollisionPoint;
        Anchor.Color = data.OverlayConfig.ColorCollisionAnchor;
        Anchor.UsingColor = data.OverlayConfig.UsingColorCollisionAnchor;
        SnapPointRadius = data.OverlayConfig.RadiusCollisionSnapPoint;
        SnapPointColor = data.OverlayConfig.ColorCollisionSnapPoint;

        Circle1.Draw(data);
        Circle2.Draw(data);
        if (HasAnchor) Anchor.Draw(data);

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
        (double newX, double newY) = (dragging.X, dragging.Y);
        if (Math.Abs(dragging.X - other.X) < Math.Abs(dragging.Y - other.Y))
            newX = other.X;
        else
            newY = other.Y;

        (dragging.X, dragging.Y) = (newX, newY);
    }

    private static (double, double)? SnapDrag(AbstractCollision current, DragCircle dragging, OverlayData data)
    {
        if (data.Level is null) return null;

        (double, double)? closest = data.Level.Desc.Collisions.Where(c => c != current)
            .SelectMany(c => new[] { (c.X1, c.Y1), (c.X2, c.Y2) })
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