using System;
using System.Collections.Generic;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class PressurePlateCollisionOverlay(AbstractPressurePlateCollision col) : CollisionOverlay(col)
{
    public List<DragCircle> OffsetCircles { get; set; } = [];

    public override bool Update(EditorLevel level, OverlayData data)
    {
        bool dragging = base.Update(level, data);

        col.FireOffsetX = PropertiesWindow.NormalizeFireOffset(col.TrapPowers.Length, col.FireOffsetX, 0);
        bool sharedFireOffsetX = col.FireOffsetX.Length == 1;
        col.FireOffsetY = PropertiesWindow.NormalizeFireOffset(col.TrapPowers.Length, col.FireOffsetY, -10);
        bool sharedFireOffsetY = col.FireOffsetY.Length == 1;

        int neededCircles = Math.Max(col.FireOffsetX.Length, col.FireOffsetY.Length);
        // too many circles
        while (neededCircles < OffsetCircles.Count)
            OffsetCircles.RemoveAt(OffsetCircles.Count - 1);
        // too few circles
        while (neededCircles > OffsetCircles.Count)
            OffsetCircles.Add(new(0, 0));

        (double offsetX, double offsetY) = (0, 0);
        (double dynOffsetX, double dynOffsetY) = (0, 0);
        if (col.Parent is not null)
        {
            (dynOffsetX, dynOffsetY) = col.Parent.GetOffset(data.Context);
            (offsetX, offsetY) = (dynOffsetX + col.Parent.X, dynOffsetY + col.Parent.Y);
        }

        for (int i = 0; i < OffsetCircles.Count; ++i)
        {
            DragCircle circle = OffsetCircles[i];
            circle.Radius = data.OverlayConfig.RadiusFireOffset;
            double oldX = sharedFireOffsetX ? col.FireOffsetX[0] : col.FireOffsetX[i];
            double oldY = sharedFireOffsetY ? col.FireOffsetY[0] : col.FireOffsetY[i];
            circle.X = oldX + offsetX;
            circle.Y = oldY + offsetY;
            circle.Update(level.Camera, data, !dragging);

            if (circle.Dragging)
            {
                dragging = true;

                int iCapture = i;
                double newX = Math.Round(circle.X - offsetX, ROUND_DECIMALS);
                double newY = Math.Round(circle.Y - offsetY, ROUND_DECIMALS);
                level.CommandHistory.Add(new PropChangeCommand<double, double>((val1, val2) =>
                {
                    col.FireOffsetX[sharedFireOffsetX ? 0 : iCapture] = val1;
                    col.FireOffsetY[sharedFireOffsetY ? 0 : iCapture] = val2;
                },
                oldX, oldY,
                newX, newY
                ));
            }
        }

        return dragging;
    }

    public override void Draw(EditorLevel level, OverlayData data)
    {
        base.Draw(level, data);

        foreach (DragCircle circle in OffsetCircles)
        {
            circle.Color = data.OverlayConfig.ColorFireOffset;
            circle.UsingColor = data.OverlayConfig.UsingColorFireOffset;
            circle.Draw(data);
            (double midX, double midY) = ((Circle1.X + Circle2.X) / 2, (Circle1.Y + Circle2.Y) / 2);
            Rl.DrawLineV(new((float)midX, (float)midY), new((float)circle.X, (float)circle.Y), data.OverlayConfig.ColorFireOffsetLine);
            (double arrowEndX, double arrowEndY) = (circle.X + (col.FaceLeft ? -1 : 1) * data.OverlayConfig.LengthFireDirectionArrow, circle.Y);
            WmeUtils.DrawArrow(circle.X, circle.Y, arrowEndX, arrowEndY, data.OverlayConfig.OffsetFireDirectionArrowSide, data.OverlayConfig.OffsetFireDirectionArrowBack, data.OverlayConfig.ColorFireDirectionArrow);
        }
    }
}