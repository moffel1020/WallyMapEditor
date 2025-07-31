using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class AbstractDynamicOverlay<T>(AbstractDynamic<T> dyn) : IOverlay
    where T : ISerializable, IDrawable
{
    public DragCircle Position { get; set; } = new(dyn.X, dyn.Y);

    public void Draw(EditorLevel level, OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorDynamicPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorDynamicPosition;

        Position.Draw(data);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        Position.Radius = data.OverlayConfig.RadiusDynamicPosition;

        (double offsetX, double offsetY) = dyn.GetOffset(data.Context);
        (Position.X, Position.Y) = (dyn.X + offsetX, dyn.Y + offsetY);
        Position.Update(level.Camera, data, true);

        if (Position.Dragging)
        {
            level.CommandHistory.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (dyn.X, dyn.Y) = (val1, val2),
                dyn.X, dyn.Y,
                Position.X - offsetX, Position.Y - offsetY
            ));
        }

        return Position.Dragging;
    }
}