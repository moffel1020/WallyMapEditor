using System;
using System.Collections.Generic;
using WallyMapSpinzor2;

/*
TODO:
editing the fire offset is much better when you see the line from the collision to the fire offset
this means we need to somehow temporarily override the render config when opening the overlay...

we don't want to fully disable fire offset rendering from the library, since it could be useful to look at all the pressure plates in the map
*/

namespace WallyMapEditor;

public class PressurePlateCollisionOverlay(AbstractPressurePlateCollision col) : CollisionOverlay(col)
{
    public List<DragCircle> OffsetCircles { get; set; } = [];

    public override bool Update(OverlayData data, CommandHistory cmd)
    {
        bool dragging = base.Update(data, cmd);

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
            circle.Update(data, !dragging);

            if (circle.Dragging)
            {
                dragging = true;

                int iCapture = i;
                cmd.Add(new PropChangeCommand<(double, double)>(val =>
                {
                    col.FireOffsetX[sharedFireOffsetX ? 0 : iCapture] = val.Item1;
                    col.FireOffsetY[sharedFireOffsetY ? 0 : iCapture] = val.Item2;
                },
                (oldX, oldY),
                (Math.Round(circle.X - offsetX, ROUND_DECIMALS), Math.Round(circle.Y - offsetY, ROUND_DECIMALS))
                ));
            }
        }

        return dragging;
    }

    public override void Draw(OverlayData data)
    {
        base.Draw(data);

        foreach (DragCircle circle in OffsetCircles)
        {
            circle.Color = data.OverlayConfig.ColorFireOffset;
            circle.UsingColor = data.OverlayConfig.UsingColorFireOffset;
            circle.Draw(data);
        }
    }
}