using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class MovingPlatformOverlay(MovingPlatform plat) : IOverlay
{
    public DragCircle Position { get; set; } = new(plat.X, plat.Y);
    public DragCircle Center { get; set; } = new(plat.Animation.CenterX ?? 0, plat.Animation.CenterY ?? 0);

    public Dictionary<KeyFrame, KeyFrameOverlay> KeyFrameCircles { get; set; } = [];

    public void Draw(EditorLevel level, OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorMovingPlatformPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorMovingPlatformPosition;
        Position.Draw(data);

        if (plat.Animation.CenterX is not null || plat.Animation.CenterY is not null)
        {
            Center.Color = data.OverlayConfig.ColorAnmCenter;
            Center.UsingColor = data.OverlayConfig.ColorAnmCenter;
            Center.Draw(data);
            RaylibEx.DrawCenteredText(
                "C",
                (plat.Animation.CenterX ?? 0) + plat.X,
                (plat.Animation.CenterY ?? 0) + plat.Y,
                Center.Radius,
                data.OverlayConfig.FontSizeAnmCenter,
                data.OverlayConfig.TextColorAnmCenter
            );
        }

        // draw higher framenum keyframes ontop of lower framenum keyframes
        foreach (KeyFrameOverlay kfo in KeyFrameCircles.Values.OrderBy(k => k.FrameNumOverride))
            kfo.Draw(level, data);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        Position.Radius = data.OverlayConfig.RadiusMovingPlatformPosition;
        Center.Radius = data.OverlayConfig.RadiusAnmCenter;

        bool dragging = false;

        if (plat.Animation.CenterX is not null || plat.Animation.CenterY is not null)
        {
            Center.X = (plat.Animation.CenterX ?? 0) + plat.X;
            Center.Y = (plat.Animation.CenterY ?? 0) + plat.Y;
            /*Center.Update(level.Camera, data, !dragging);
            dragging |= Center.Dragging;

            if (Center.Dragging)
            {
                level.CommandHistory.Add(new PropChangeCommand<double?, double?>(
                    (val1, val2) => (plat.Animation.CenterX, plat.Animation.CenterY) = (val1, val2),
                    plat.Animation.CenterX, plat.Animation.CenterY,
                    // only update the center if it's not null
                    plat.Animation.CenterX is not null ? Center.X - plat.X : null,
                    plat.Animation.CenterY is not null ? Center.Y - plat.Y : null
                ));
            }*/
        }

        HashSet<KeyFrame> currentKeyFrames = [];
        // we go through the keyframes in the reverse order
        // this gives higher framenum keyframes priority
        foreach ((KeyFrame kf, int num) in EnumerateKeyFrames(plat.Animation.KeyFrames).Reverse())
        {
            currentKeyFrames.Add(kf);
            if (!KeyFrameCircles.TryGetValue(kf, out KeyFrameOverlay? kfo))
                kfo = KeyFrameCircles[kf] = new(kf);
            kfo.PlatOffset = (plat.X, plat.Y);
            kfo.AllowDragging = !dragging;
            kfo.FrameNumOverride = num;
            kfo.CenterXFallback = plat.Animation.CenterX;
            kfo.CenterYFallback = plat.Animation.CenterY;
            dragging |= kfo.Update(level, data);
        }
        // remove deleted keyframes
        foreach (KeyFrame kf in KeyFrameCircles.Keys)
        {
            if (!currentKeyFrames.Contains(kf))
                KeyFrameCircles.Remove(kf);
        }

        if (!data.Context.PlatIDMovingPlatformTransform.TryGetValue(plat.PlatID, out Transform platTransform))
            throw new Exception($"Attempt to update overlay for moving platform with PlatID {plat.PlatID}, but moving platform offset dictionary did not contain that PlatID");
        (double offsetX, double offsetY) = platTransform * (0, 0);
        (Position.X, Position.Y) = (offsetX, offsetY);
        Position.Update(level.Camera, data, !dragging);

        if (Position.Dragging)
        {
            level.CommandHistory.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (plat.X, plat.Y) = (val1, val2),
                plat.X, plat.Y,
                plat.X + Position.X - offsetX, plat.Y + Position.Y - offsetY
            ));
        }

        return Position.Dragging || dragging;
    }

    private static IEnumerable<(KeyFrame, int)> EnumerateKeyFrames(IEnumerable<AbstractKeyFrame> abstractKeyFrames, int parentNum = 0)
    {
        foreach (AbstractKeyFrame akf in abstractKeyFrames)
        {
            if (akf is KeyFrame kf)
            {
                yield return (kf, parentNum + kf.FrameNum);
            }
            else if (akf is Phase p)
            {
                foreach ((KeyFrame kf2, int num) in EnumerateKeyFrames(p.KeyFrames, parentNum + p.StartFrame))
                    yield return (kf2, num);
            }
            else
            {
                throw new Exception($"Unknown keyframe type {akf.GetType().Name}");
            }
        }
    }
}