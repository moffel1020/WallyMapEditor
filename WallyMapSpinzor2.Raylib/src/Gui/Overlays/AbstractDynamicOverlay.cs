using System;

namespace WallyMapSpinzor2.Raylib;

public class AbstracyDynamicOverlay<T>(AbstractDynamic<T> dyn) : IOverlay
    where T : ISerializable, IDeserializable, IDrawable
{
    public DragCircle Position { get; set; } = new(dyn.X, dyn.Y);

    public void Draw(OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorDynamicPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorDynamicPosition;

        Position.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        Position.Radius = data.OverlayConfig.RadiusDynamicPosition;

        (double offsetX, double offsetY) = dyn.GetOffset(data.Context);
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