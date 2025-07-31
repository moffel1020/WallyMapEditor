using System;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class VolumeOverlay(AbstractVolume volume) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(volume.X, volume.Y, volume.W, volume.H);

    public void Draw(EditorLevel level, OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorVolumeBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorVolumeBox;

        ResizableBox.Draw(data);
    }

    private static (int, int, int, int) RoundBoundsToInt((double, double, double, double) val) =>
        ((int)Math.Round(val.Item1), (int)Math.Round(val.Item2), (int)Math.Round(val.Item3), (int)Math.Round(val.Item4));

    public bool Update(EditorLevel level, OverlayData data)
    {
        CommandHistory cmd = level.CommandHistory;

        ResizableBox.CircleRadius = data.OverlayConfig.RadiusVolumeCorner;

        ResizableBox.Update(level.Camera, data, volume.X, volume.Y, volume.W, volume.H);
        (int x, int y, int w, int h) = RoundBoundsToInt(ResizableBox.Bounds);

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<int, int, int, int>(
                (val1, val2, val3, val4) => (volume.X, volume.Y, volume.W, volume.H) = (val1, val2, val3, val4),
                volume.X, volume.Y, volume.W, volume.H,
                x, y, w, h
            ));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<int, int>(
                (val1, val2) => (volume.X, volume.Y) = (val1, val2),
                volume.X, volume.Y,
                x, y
            ));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}