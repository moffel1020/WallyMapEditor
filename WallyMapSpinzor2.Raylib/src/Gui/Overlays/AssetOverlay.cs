using System;
namespace WallyMapSpinzor2.Raylib;

public class AssetOverlay(AbstractAsset asset) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(0, 0);
    public DragCircle TopRight { get; set; } = new(0, 0);
    public DragCircle BotLeft { get; set; } = new(0, 0);
    public DragCircle BotRight { get; set; } = new(0, 0);
    public DragBox MoveRect { get; set; } = new(0, 0, 0, 0);

    public void Draw(OverlayData data)
    {
        MoveRect.Color = TopLeft.Color = TopRight.Color = BotLeft.Color = BotRight.Color = data.OverlayConfig.ColorAssetBox;
        MoveRect.UsingColor = TopLeft.UsingColor = TopRight.UsingColor = BotLeft.UsingColor = BotRight.UsingColor = data.OverlayConfig.UsingColorAssetBox;

        TopLeft.Draw(data);
        TopRight.Draw(data);
        BotLeft.Draw(data);
        BotRight.Draw(data);
        MoveRect.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        if (asset.AssetName is null) throw new ArgumentException("AssetOverlay used on asset without AssetName");

        TopLeft.Radius = TopRight.Radius = BotLeft.Radius = BotRight.Radius = data.OverlayConfig.RadiusAssetCorner;

        Transform trans = FullTransform(asset, data.Context);
        Transform inv = Transform.CreateInverse(trans);

        (TopLeft.X, TopLeft.Y) = (trans.TranslateX, trans.TranslateY);
        (TopRight.X, TopRight.Y) = trans * (asset.W!.Value, 0);
        (BotLeft.X, BotLeft.Y) = trans * (0, asset.H!.Value);
        (BotRight.X, BotRight.Y) = trans * (asset.W!.Value, asset.H!.Value);

        MoveRect.Transform = trans;
        (MoveRect.X, MoveRect.Y) = (0, 0);
        (MoveRect.W, MoveRect.H) = (asset.W.Value, asset.H.Value);

        bool dragging = UpdateCircles(data, trans, inv);

        TransfromDragCircles(inv);
        (double newW, double newH) = (TopRight.X - TopLeft.X, BotLeft.Y - TopLeft.Y);
        (double newX, double newY) = asset.Transform * TopLeft.Position;
        TransfromDragCircles(trans);

        if (dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double, double, double)>(
                val => (asset.X, asset.Y, asset.W, asset.H) = val,
                (asset.X, asset.Y, asset.W.Value, asset.H.Value),
                (newX, newY, newW, newH)));
        }

        MoveRect.Update(data, !dragging);

        if (MoveRect.Dragging)
        {
            cmd.Add(new PropChangeCommand<(double, double)>(
                val => (asset.X, asset.Y) = val,
                (asset.X, asset.Y),
                asset.Transform * (MoveRect.X, MoveRect.Y)));
        }

        return dragging || MoveRect.Dragging;
    }

    private bool UpdateCircles(OverlayData data, Transform trans, Transform invTrans)
    {
        TopLeft.Update(data, true);
        bool dragging = TopLeft.Dragging;
        TopRight.Update(data, !dragging);
        dragging |= TopRight.Dragging;
        BotLeft.Update(data, !dragging);
        dragging |= BotLeft.Dragging;
        BotRight.Update(data, !dragging);
        dragging |= BotRight.Dragging;

        TransfromDragCircles(invTrans);
        if (TopLeft.Dragging)
        {
            BotLeft.X = TopLeft.X;
            TopRight.Y = TopLeft.Y;
        }
        else if (TopRight.Dragging)
        {
            BotRight.X = TopRight.X;
            TopLeft.Y = TopRight.Y;
        }
        else if (BotLeft.Dragging)
        {
            TopLeft.X = BotLeft.X;
            BotRight.Y = BotLeft.Y;
        }
        else if (BotRight.Dragging)
        {
            TopRight.X = BotRight.X;
            BotLeft.Y = BotRight.Y;
        }
        TransfromDragCircles(trans);

        return dragging;
    }

    private static Transform FullTransform(AbstractAsset? a, RenderContext context) => a switch
    {
        MovingPlatform m => GetMovingPlatformTransform(m, context),
        AbstractAsset => FullTransform(a.Parent, context) * a.Transform,
        null => Transform.IDENTITY
    };

    private static Transform GetMovingPlatformTransform(MovingPlatform m, RenderContext context)
    {
        if (context.PlatIDMovingPlatformOffset.TryGetValue(m.PlatID, out (double, double) offset))
            return Transform.CreateTranslate(offset.Item1, offset.Item2);

        return Transform.IDENTITY;
    }

    private void TransfromDragCircles(Transform trans)
    {
        (TopLeft.X, TopLeft.Y) = trans * TopLeft.Position;
        (TopRight.X, TopRight.Y) = trans * TopRight.Position;
        (BotLeft.X, BotLeft.Y) = trans * BotLeft.Position;
        (BotRight.X, BotRight.Y) = trans * BotRight.Position;
    }
}