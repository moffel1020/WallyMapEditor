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

    public virtual bool Update(EditorLevel level, OverlayData data)
    {
        CommandHistory cmd = level.CommandHistory;
        Camera2D cam = level.Camera;

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

        Circle1.Update(cam, data, true);
        Circle2.Update(cam, data, !Circle1.Dragging);
        if (HasAnchor) Anchor.Update(cam, data, !Circle1.Dragging && !Circle2.Dragging);
        Center.Update(cam, data, !Circle1.Dragging && !Circle2.Dragging && !Anchor.Dragging);

        if (Circle1.Dragging)
        {
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
                LockAxisDrag(Circle1, Circle2);

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
                _snapToPoint = SnapDrag(level, col, Circle1, data);

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
                _snapToPoint = SnapDrag(level, col, Circle2, data);

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
            if (Rl.IsKeyDown(KeyboardKey.LeftShift))
            {
                _centerDragOrigin ??= (Center.X, Center.Y);
                LockAxisDrag(Center, _centerDragOrigin.Value.Item1, _centerDragOrigin.Value.Item2);
            }
            else
            {
                _centerDragOrigin = null;
            }

            double diffX = col.X2 - col.X1;
            double diffY = col.Y2 - col.Y1;

            _snapToPoint = null;
            if (!Rl.IsKeyDown(KeyboardKey.LeftAlt))
            {
                // we should not trust the Circle1 and Circle2 positions here,
                // because Center.Update did not update them.
                // so recalculate.
                double x1 = Center.X - diffX / 2;
                double y1 = Center.Y - diffY / 2;
                double x2 = Center.X + diffX / 2;
                double y2 = Center.Y + diffY / 2;

                // find closest collision
                (double x, double y)? closest1 = GetClosestCollisionPoint(level, col, x1, y1, data);
                // calculate distance
                double? distance1 = closest1 is null ? null : DistanceSquared(x1, y1, closest1.Value.x, closest1.Value.y);

                // find closest collision
                (double x, double y)? closest2 = GetClosestCollisionPoint(level, col, x2, y2, data);
                // calculate distance
                double? distance2 = closest2 is null ? null : DistanceSquared(x2, y2, closest2.Value.x, closest2.Value.y);

                // snap point is whichever is closer
                if (distance2 is null)
                    _snapToPoint = closest1;
                else if (distance1 is null)
                    _snapToPoint = closest2;
                else if (distance1.Value > distance2.Value)
                    _snapToPoint = closest2;
                else
                    _snapToPoint = closest1;

                // if distance is too far away, discard
                if (distance1 is not null && distance1.Value > MAX_SNAP_DISTANCE)
                {
                    closest1 = null;
                    distance1 = null;
                }
                if (distance2 is not null && distance2.Value > MAX_SNAP_DISTANCE)
                {
                    closest2 = null;
                    distance2 = null;
                }

                // if both are good, use whichever is better
                if (distance1 is not null && distance2 is not null)
                {
                    // 1 is better
                    if (distance1.Value <= distance2.Value)
                    {
                        closest2 = null;
                        distance2 = null;
                    }
                    // 2 is better
                    else
                    {
                        closest1 = null;
                        distance1 = null;
                    }
                }

                // do the snapping
                if (closest1 is not null)
                {
                    (double snapX1, double snapY1) = closest1.Value;
                    Center.X += snapX1 - x1;
                    Center.Y += snapY1 - y1;
                }
                else if (closest2 is not null)
                {
                    (double snapX2, double snapY2) = closest2.Value;
                    Center.X += snapX2 - x2;
                    Center.Y += snapY2 - y2;
                }
            }

            // round
            double centerX = Math.Round(Center.X - offsetX, ROUND_DECIMALS);
            double centerY = Math.Round(Center.Y - offsetY, ROUND_DECIMALS);
            // recalculate based on center
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

    public virtual void Draw(EditorLevel level, OverlayData data)
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
            if (
                ((Center.Dragging || Circle1.Dragging) && DistanceSquared(px, py, Circle1.X, Circle1.Y) < SNAP_POINT_VISIBLE_DISTANCE) ||
                ((Center.Dragging || Circle2.Dragging) && DistanceSquared(px, py, Circle2.X, Circle2.Y) < SNAP_POINT_VISIBLE_DISTANCE)
            )
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

    private static (double, double)? SnapDrag(EditorLevel level, AbstractCollision current, DragCircle dragging, OverlayData data)
    {
        (double, double)? closest = GetClosestCollisionPoint(level, current, dragging.X, dragging.Y, data);

        if (closest is not null && DistanceSquared(dragging.X, dragging.Y, closest.Value.Item1, closest.Value.Item2) <= MAX_SNAP_DISTANCE)
            (dragging.X, dragging.Y) = (closest.Value.Item1, closest.Value.Item2);

        return closest;
    }

    private static (double, double)? GetClosestCollisionPoint(EditorLevel level, AbstractCollision current, double x, double y, OverlayData data)
    {
        LevelDesc ld = level.Level.Desc;
        IEnumerable<(double, double)> otherPoints = ld.Collisions.Where(c => c != current)
            .SelectMany(IEnumerable<(double, double)> (c) => [(c.X1, c.Y1), (c.X2, c.Y2)])
            .Concat(CollisionPointsAbsolute(ld.DynamicCollisions, data.Context, current));

        return otherPoints.Any() ? otherPoints.MinBy(p => DistanceSquared(x, y, p.Item1, p.Item2)) : null;
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