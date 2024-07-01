using System;

namespace WallyMapSpinzor2.Raylib;

public class AbstracyDynamicOverlay<T, U>(T dyn) : IOverlay
    where T : AbstractDynamic<U>
    where U : ISerializable, IDeserializable, IDrawable
{
    public DragCircle Position { get; set; } = new(dyn.X, dyn.Y)
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
        if (!data.Context.PlatIDDynamicOffset.TryGetValue(dyn.PlatID, out (double, double) dynOffset))
            throw new Exception($"Attempt to draw overlay for dynamic object with PlatID {dyn.PlatID}, but dynamic offset dictionary did not contain that PlatID");
        (double offsetX, double offsetY) = dynOffset;
        (Position.X, Position.Y) = (dyn.X + offsetX, dyn.Y + offsetY);
        Position.Update(data, true);

        if (Position.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (dyn.X, dyn.Y) = val,
                (dyn.X, dyn.Y),
                (Position.X - offsetX, Position.Y - offsetY)));
        }

        return Position.Dragging;
    }
}