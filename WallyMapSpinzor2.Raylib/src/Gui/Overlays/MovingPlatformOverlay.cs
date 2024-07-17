using System;
using System.Collections.Generic;
using System.Linq;

namespace WallyMapSpinzor2.Raylib;

public class MovingPlatformOverlay(MovingPlatform plat) : IOverlay
{
    public DragCircle Position { get; set; } = new(plat.X, plat.Y);

    public Dictionary<KeyFrame, KeyFrameOverlay> KeyFrameCircles { get; set; } = [];

    public void Draw(OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorMovingPlatformPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorMovingPlatformPosition;

        Position.Draw(data);
        foreach (KeyFrameOverlay kfo in KeyFrameCircles.Values)
            kfo.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        Position.Radius = data.OverlayConfig.RadiusMovingPlatformPosition;

        bool dragging = false;
        foreach ((KeyFrame kf, int num) in EnumerateKeyFrames(plat.Animation.KeyFrames))
        {
            if (!KeyFrameCircles.ContainsKey(kf))
                KeyFrameCircles[kf] = new(kf);

            KeyFrameOverlay kfo = KeyFrameCircles[kf];
            kfo.PlatOffset = (plat.X, plat.Y);
            kfo.AllowDragging = !dragging;
            kfo.FrameNumOverride = num;
            dragging |= kfo.Update(data, cmd);
        }

        if (!data.Context.PlatIDMovingPlatformOffset.TryGetValue(plat.PlatID, out (double, double) platOffset))
            throw new Exception($"Attempt to update overlay for moving platform with PlatID {plat.PlatID}, but moving platform offset dictionary did not contain that PlatID");
        (double offsetX, double offsetY) = platOffset;
        (Position.X, Position.Y) = (offsetX, offsetY);
        Position.Update(data, !dragging);

        if (Position.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (plat.X, plat.Y) = val,
                (plat.X, plat.Y),
                (plat.X + Position.X - offsetX, plat.Y + Position.Y - offsetY)));
        }

        return Position.Dragging || dragging;
    }

    private static IEnumerable<(KeyFrame, int)> EnumerateKeyFrames(IEnumerable<AbstractKeyFrame> abstractKeyFrames)
    {
        Queue<(AbstractKeyFrame, int)> toProcess = new(abstractKeyFrames.Zip(Enumerable.Repeat(0, abstractKeyFrames.Count())));

        while (toProcess.Count > 0)
        {
            (AbstractKeyFrame akf, int num) = toProcess.Dequeue();
            if (akf is KeyFrame kf)
            {
                yield return (kf, num + kf.FrameNum);
            }
            else if (akf is Phase p)
            {
                foreach (AbstractKeyFrame child in p.KeyFrames)
                    toProcess.Enqueue((child, p.StartFrame + num));
            }
            else
            {
                throw new Exception($"Unknown keyframe type {akf.GetType().Name}");
            }
        }
    }
}