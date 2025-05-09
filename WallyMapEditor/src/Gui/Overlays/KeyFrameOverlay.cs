using System.Numerics;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class KeyFrameOverlay(KeyFrame kf) : IOverlay
{
    public (double, double) PlatOffset { get; set; }
    public bool AllowDragging { get; set; } = true;

    public DragCircle Circle { get; set; } = new(kf.X, kf.Y);

    public int? FrameNumOverride { get; set; }

    public void Draw(EditorLevel level, OverlayData data)
    {
        int fontSize = data.OverlayConfig.FontSizeKeyFrameNum;
        RlColor textColor = data.OverlayConfig.TextColorKeyFrameNum;
        Circle.Color = data.OverlayConfig.ColorKeyFramePosition;
        Circle.UsingColor = data.OverlayConfig.UsingColorKeyFramePosition;

        (double offX, double offY) = PlatOffset;
        string frameNum = (FrameNumOverride ?? kf.FrameNum).ToString();
        float textW = RaylibEx.MeasureTextV(frameNum, fontSize).X;

        Circle.Draw(data);
        double textX = kf.X + offX - textW / 2;
        double textY = kf.Y + offY - Circle.Radius / 2;
        Vector2 textPos = new((float)textX, (float)textY);
        RaylibEx.DrawTextV(frameNum, textPos, fontSize, textColor);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        Circle.Radius = data.OverlayConfig.RadiusKeyFramePosition;

        (double offX, double offY) = PlatOffset;
        Circle.X = kf.X + offX;
        Circle.Y = kf.Y + offY;

        Circle.Update(level.Camera, data, AllowDragging);

        if (Circle.Dragging)
        {
            level.CommandHistory.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (kf.X, kf.Y) = (val1, val2),
                kf.X, kf.Y,
                Circle.X - offX, Circle.Y - offY
            ));
        }

        return Circle.Dragging;
    }
}