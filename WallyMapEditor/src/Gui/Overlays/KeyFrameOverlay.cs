using System.Numerics;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class KeyFrameOverlay(KeyFrame kf) : IOverlay
{
    public (double, double) PlatOffset { get; set; }
    public bool AllowDragging { get; set; } = true;

    public DragCircle Circle { get; set; } = new(kf.X, kf.Y);
    public DragCircle Center { get; set; } = new(kf.CenterX ?? 0, kf.CenterY ?? 0);

    public int? FrameNumOverride { get; set; }
    public double? CenterXFallback { get; set; }
    public double? CenterYFallback { get; set; }

    public void Draw(EditorLevel level, OverlayData data)
    {
        (double offX, double offY) = PlatOffset;

        // pos circle
        Circle.Color = data.OverlayConfig.ColorKeyFramePosition;
        Circle.UsingColor = data.OverlayConfig.UsingColorKeyFramePosition;
        Circle.Draw(data);
        // pos text
        int numFontSize = data.OverlayConfig.FontSizeKeyFrameNum;
        RlColor numTextColor = data.OverlayConfig.TextColorKeyFrameNum;
        string frameNumText = (FrameNumOverride ?? kf.FrameNum).ToString();
        float frameNumTextW = RaylibEx.MeasureTextV(frameNumText, numFontSize).X;
        double frameNumTextX = kf.X + offX - frameNumTextW / 2;
        double frameNumTextY = kf.Y + offY - Circle.Radius / 2;
        Vector2 frameNumTextPos = new((float)frameNumTextX, (float)frameNumTextY);
        RaylibEx.DrawTextV(frameNumText, frameNumTextPos, numFontSize, numTextColor);

        /*
        notice the fallback isn't taken into account.
        this is because if both of these are null,
        then the fallback is actually the center of the Animation.
        */
        if (kf.CenterX is not null || kf.CenterY is not null)
        {
            // center circle
            Center.Color = data.OverlayConfig.ColorKeyFrameCenter;
            Center.UsingColor = data.OverlayConfig.UsingColorKeyFrameCenter;
            Center.Draw(data);
            // center text
            int centerFontSize = data.OverlayConfig.FontSizeKeyFrameCenter;
            RlColor centerTextColor = data.OverlayConfig.TextColorKeyFrameCenter;
            string centerText = $"C{frameNumText}";
            float centerTextW = RaylibEx.MeasureTextV(centerText, centerFontSize).X;
            double centerTextX = (kf.CenterX ?? CenterXFallback ?? 0) + offX - centerTextW / 2;
            double centerTextY = (kf.CenterY ?? CenterYFallback ?? 0) + offY - Circle.Radius / 2;
            Vector2 centerTextPos = new((float)centerTextX, (float)centerTextY);
            RaylibEx.DrawTextV(centerText, centerTextPos, centerFontSize, centerTextColor);
        }
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        bool dragging = false;

        (double offX, double offY) = PlatOffset;

        if (kf.CenterX is not null || kf.CenterY is not null)
        {
            Center.X = (kf.CenterX ?? CenterXFallback ?? 0) + offX;
            Center.Y = (kf.CenterY ?? CenterYFallback ?? 0) + offY;
            Center.Radius = data.OverlayConfig.RadiusKeyFrameCenter;
        }

        Circle.X = kf.X + offX;
        Circle.Y = kf.Y + offY;
        Circle.Radius = data.OverlayConfig.RadiusKeyFramePosition;
        Circle.Update(level.Camera, data, AllowDragging && !dragging);
        dragging |= Circle.Dragging;
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