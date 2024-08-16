using WallyMapSpinzor2;

namespace WallyMapEditor;

public class KeyFrameOverlay(KeyFrame kf) : IOverlay
{
    public (double, double) PlatOffset { get; set; }
    public bool AllowDragging { get; set; } = true;

    public DragCircle Circle { get; set; } = new(kf.X, kf.Y);

    public int? FrameNumOverride { get; set; }

    public void Draw(OverlayData data)
    {
        int fontSize = data.OverlayConfig.FontSizeKeyFrameNum;
        RlColor textColor = data.OverlayConfig.TextColorKeyFrameNum;
        Circle.Color = data.OverlayConfig.ColorKeyFramePosition;
        Circle.UsingColor = data.OverlayConfig.UsingColorKeyFramePosition;

        (double offX, double offY) = PlatOffset;
        string frameNum = (FrameNumOverride ?? kf.FrameNum).ToString();
        int textW = Rl.MeasureText(frameNum, fontSize);

        Circle.Draw(data);
        Rl.DrawText(frameNum, (int)(kf.X + offX - textW / 2), (int)(kf.Y + offY - Circle.Radius / 2), fontSize, textColor);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        Circle.Radius = data.OverlayConfig.RadiusKeyFramePosition;

        (double offX, double offY) = PlatOffset;
        Circle.X = kf.X + offX;
        Circle.Y = kf.Y + offY;

        Circle.Update(data, AllowDragging);

        if (Circle.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (kf.X, kf.Y) = val,
                (kf.X, kf.Y),
                (Circle.X - offX, Circle.Y - offY)));
        }

        return Circle.Dragging;
    }
}