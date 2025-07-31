using System;
using System.Collections.Generic;
using System.Linq;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class MovingPlatformOverlay(MovingPlatform plat) : IOverlay
{
    public DragCircle Position { get; set; } = new(plat.X, plat.Y);

    public Dictionary<KeyFrame, KeyFrameOverlay> KeyFrameCircles { get; set; } = [];

    public void Draw(EditorLevel level, OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorMovingPlatformPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorMovingPlatformPosition;

        Position.Draw(data);
        // draw higher framenum keyframes ontop of lower framenum keyframes
        foreach (KeyFrameOverlay kfo in KeyFrameCircles.Values.OrderBy(k => k.FrameNumOverride))
            kfo.Draw(level, data);
    }

    public bool Update(EditorLevel level, OverlayData data)
    {
        Position.Radius = data.OverlayConfig.RadiusMovingPlatformPosition;

        bool dragging = false;
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