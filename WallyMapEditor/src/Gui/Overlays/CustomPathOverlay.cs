using System.Collections.Generic;
using System.Numerics;
using WallyMapSpinzor2;

/*
TODO:
editing the fire offset is much better when you see the line from the collision to the fire offset
this means we need to somehow temporarily override the render config when opening the overlay...

we don't want to fully disable fire offset rendering from the library, since it could be useful to look at all the pressure plates in the map
*/

namespace WallyMapEditor;

public class CustomPathOverlay(CustomPath cp) : IOverlay
{
    public List<DragCircle> Points { get; set; } = [];

    public bool Update(EditorLevel level, OverlayData data)
    {
        bool dragging = false;

        int neededCircles = cp.Points.Length;
        // too many circles
        while (neededCircles < Points.Count)
            Points.RemoveAt(Points.Count - 1);
        // too few circles
        while (neededCircles > Points.Count)
            Points.Add(new(0, 0));

        for (int i = 0; i < Points.Count; ++i)
        {
            Point point = cp.Points[i];
            DragCircle circle = Points[i];
            circle.Radius = data.OverlayConfig.RadiusPathPoint;
            circle.X = point.X;
            circle.Y = point.Y;
            circle.Update(level.Camera, data, !dragging);

            if (circle.Dragging)
            {
                dragging = true;
                level.CommandHistory.Add(new PropChangeCommand<double, double>(
                    (val1, val2) => (point.X, point.Y) = (val1, val2),
                    point.X, point.Y,
                    circle.X, circle.Y
                ));
            }
        }

        return dragging;
    }

    public void Draw(EditorLevel level, OverlayData data)
    {
        foreach ((DragCircle circle, int i) in Points.Indexed())
        {
            circle.Color = data.OverlayConfig.ColorPathPoint;
            circle.UsingColor = data.OverlayConfig.UsingColorPathPoint;
            circle.Draw(data);

            RaylibEx.DrawCenteredText(
                i.ToString(),
                circle.X,
                circle.Y,
                circle.Radius,
                data.OverlayConfig.FontSizePathPointNum,
                data.OverlayConfig.TextColorPathPointNum
            );
        }
    }
}