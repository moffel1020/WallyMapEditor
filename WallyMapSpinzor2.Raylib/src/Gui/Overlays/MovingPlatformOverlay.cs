using System;

namespace WallyMapSpinzor2.Raylib;

public class MovingPlatformOverlay(MovingPlatform plat) : IOverlay
{
    public DragCircle Position { get; set; } = new(plat.X, plat.Y)
    {
        Radius = 70,
        Color = Raylib_cs.Color.Red with { A = 190 },
        UsingColor = Raylib_cs.Color.Pink with { A = 190 },
    };

    public void Draw(OverlayData data)
    {
        Position.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        if (!data.Context.PlatIDMovingPlatformOffset.TryGetValue(plat.PlatID, out (double, double) platOffset))
            throw new Exception($"Attempt to update overlay for moving platform with PlatID {plat.PlatID}, but moving platform offset dictionary did not contain that PlatID");
        (double offsetX, double offsetY) = platOffset;
        (Position.X, Position.Y) = (offsetX, offsetY);
        Position.Update(data, true);

        if (Position.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (plat.X, plat.Y) = val,
                (plat.X, plat.Y),
                (plat.X + Position.X - offsetX, plat.Y + Position.Y - offsetY)));
        }

        return Position.Dragging;
    }
}