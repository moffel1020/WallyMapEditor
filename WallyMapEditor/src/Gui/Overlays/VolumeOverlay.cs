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

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        ResizableBox.CircleRadius = data.OverlayConfig.RadiusVolumeCorner;

        ResizableBox.Update(data, volume.X, volume.Y, volume.W, volume.H);
        (double x, double y, double w, double h) = ResizableBox.Bounds;

        if (ResizableBox.Resizing)
        {
            cmd.Add(new PropChangeCommand<(int, int, int, int)>(
                val => (volume.X, volume.Y, volume.W, volume.H) = val,
                (volume.X, volume.Y, volume.W, volume.H),
                ((int)x, (int)y, (int)w, (int)h)));
        }

        if (ResizableBox.Moving)
        {
            cmd.Add(new PropChangeCommand<(int, int)>(
                val => (volume.X, volume.Y) = val,
                (volume.X, volume.Y),
                ((int)x, (int)y)));
        }

        return ResizableBox.Moving || ResizableBox.Resizing;
    }
}