using System;

namespace WallyMapSpinzor2.Raylib;

public class ParentAssetOverlay(AbstractAsset asset) : IOverlay
{
    public DragCircle Position { get; set; } = new(0, 0);
    public RotatePoint RotatePoint { get; set; } = new(0, 0);

    public void Draw(OverlayData data)
    {
        Position.Color = data.OverlayConfig.ColorParentAssetPosition;
        Position.UsingColor = data.OverlayConfig.UsingColorParentAssetPosition;
        RotatePoint.LineColor = data.OverlayConfig.ColorParentAssetRotationLine;

        Position.Draw(data);
        RotatePoint.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        Position.Radius = data.OverlayConfig.RadiusParentAssetPosition;

        Transform parent = AssetOverlay.FullTransform(asset.Parent, data.Context);
        (Position.X, Position.Y) = (RotatePoint.X, RotatePoint.Y) = (parent.TranslateX + asset.X, parent.TranslateY + asset.Y);

        RotatePoint.Update(data, true, asset.Rotation * Math.PI / 180);
        Position.Update(data, !RotatePoint.Active);

        if (RotatePoint.Active)
        {
            cmd.Add(new PropChangeCommand<double>(
                val => asset.Rotation = val,
                asset.Rotation,
                RotatePoint.Rotation * 180 / Math.PI));
        }

        if (Position.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (asset.X, asset.Y) = val,
                (asset.X, asset.Y),
                (Position.X - parent.TranslateX, Position.Y - parent.TranslateY)));
        }

        // TODO: maybe scaleX and scaleY

        return Position.Dragging || RotatePoint.Active;
    }
}