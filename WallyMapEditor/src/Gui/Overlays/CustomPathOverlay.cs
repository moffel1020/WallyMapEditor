using System.Collections.Generic;
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

    public bool Update(OverlayData data, CommandHistory cmd)
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
            circle.Update(data, !dragging);

            if (circle.Dragging)
            {
                dragging = true;
                cmd.Add(new PropChangeCommand<(double, double)>(val => (point.X, point.Y) = val, (point.X, point.Y), (circle.X, circle.Y)));
            }
        }

        return dragging;
    }

    public void Draw(OverlayData data)
    {
        int fontSize = data.OverlayConfig.FontSizePathPointNum;
        RlColor textColor = data.OverlayConfig.TextColorPathPointNum;

        foreach ((DragCircle circle, int i) in Points.Indexed())
        {
            int textW = Rl.MeasureText(i.ToString(), fontSize);

            circle.Color = data.OverlayConfig.ColorPathPoint;
            circle.UsingColor = data.OverlayConfig.UsingColorPathPoint;
            circle.Draw(data);

            Rl.DrawText(i.ToString(), (int)(circle.X - textW / 2), (int)(circle.Y - circle.Radius / 2), fontSize, textColor);
        }
    }
}