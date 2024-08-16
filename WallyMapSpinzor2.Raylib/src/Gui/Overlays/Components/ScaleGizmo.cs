namespace WallyMapEditor;

public class ScaleGizmo(double x, double y)
{
    public RlColor Color
    {
        get => ScaleXSlider.Color;
        set => ScaleXSlider.Color = ScaleYSlider.Color = value;
    }

    public RlColor UsingColor
    {
        get => ScaleXSlider.UsingColor;
        set => ScaleXSlider.UsingColor = ScaleYSlider.UsingColor = value;
    }

    public double Sensitivity
    {
        get => ScaleXSlider.Sensitivity;
        set => ScaleXSlider.Sensitivity = ScaleYSlider.Sensitivity = value;
    }

    public double LineWidth
    {
        get => ScaleXSlider.LineWidth;
        set => ScaleXSlider.LineWidth = ScaleYSlider.LineWidth = value;
    }

    public double Length
    {
        get => ScaleXSlider.Length;
        set => ScaleXSlider.Length = ScaleYSlider.Length = value;
    }

    public GizmoSlider ScaleXSlider { get; set; } = new(x, y);
    public GizmoSlider ScaleYSlider { get; set; } = new(x, y);

    public double ScaleX => ScaleXSlider.Value;
    public double ScaleY => ScaleYSlider.Value;

    public bool Dragging => ScaleXSlider.Dragging || ScaleYSlider.Dragging;
    public bool Hovered => ScaleXSlider.Hovered || ScaleYSlider.Hovered;

    public double Rotation
    {
        get => ScaleXSlider.Rotation;
        set => (ScaleXSlider.Rotation, ScaleYSlider.Rotation) = ((float)value, (float)value + 90);
    }

    public double X
    {
        get => ScaleXSlider.X;
        set => ScaleXSlider.X = ScaleYSlider.X = value;
    }

    public double Y
    {
        get => ScaleXSlider.Y;
        set => ScaleXSlider.Y = ScaleYSlider.Y = value;
    }

    public void Update(OverlayData data, double currentScaleX, double currentScaleY, bool allowDragging)
    {
        ScaleXSlider.Update(data, currentScaleX, allowDragging);
        ScaleYSlider.Update(data, currentScaleY, allowDragging && !ScaleXSlider.Dragging);
    }

    public void Draw(OverlayData data)
    {
        ScaleXSlider.Draw(data);
        ScaleYSlider.Draw(data);
    }
}