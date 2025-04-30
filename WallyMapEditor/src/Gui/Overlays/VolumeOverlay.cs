using System;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class VolumeOverlay(AbstractVolume volume) : IOverlay
{
    public ResizableDragBox ResizableBox { get; set; } = new(volume.X, volume.Y, volume.W, volume.H);

    public void Draw(OverlayData data)
    {
        ResizableBox.Color = data.OverlayConfig.ColorVolumeBox;
        ResizableBox.UsingColor = data.OverlayConfig.UsingColorVolumeBox;

        ResizableBox.Draw(data);
    }

    private static (int, int, int, int) RoundBoundsToInt((double, double, double, double) val) =>
        ((int)Math.Round(val.Item1), (int)Math.Round(val.Item2), (int)Math.Round(val.Item3), (int)Math.Round(val.Item4));

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        ResizableBox.CircleRadius = data.OverlayConfig.RadiusVolumeCorner;

        ResizableBox.Update(data, volume.X, volume.Y, volume.W, volume.H);
        (int x, int y, int w, int h) = RoundBoundsToInt(ResizableBox.Bounds);

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<(int, int, int, int)>(
                val => (volume.X, volume.Y, volume.W, volume.H) = val,
                (volume.X, volume.Y, volume.W, volume.H),
                (x, y, w, h)
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