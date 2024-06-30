using System;
using System.Numerics;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class AssetOverlay(AbstractAsset asset) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(0, 0);
    public DragCircle TopRight { get; set; } = new(0, 0);
    public DragCircle BotLeft { get; set; } = new(0, 0);
    public DragCircle BotRight { get; set; } = new(0, 0);

    public void Draw(OverlayData data)
    {
        TopLeft.Draw(data);
        TopRight.Draw(data);
        BotLeft.Draw(data);
        BotRight.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        if (asset.AssetName is null) throw new ArgumentException("AssetOverlay used on asset without AssetName");

        Transform trans = FullTransform(asset, data.Context); 
        Transform inv = Transform.CreateInverse(trans);

        (TopLeft.X, TopLeft.Y) = (trans.TranslateX, trans.TranslateY);
        (TopRight.X, TopRight.Y) = trans * (asset.W!.Value, 0);
        (BotLeft.X, BotLeft.Y) = trans * (0, asset.H!.Value);
        (BotRight.X, BotRight.Y) = trans * (asset.W!.Value, asset.H!.Value);

        TransfromDragCircles(inv);

        Vector2 worldPosVec = data.Viewport.ScreenToWorld(Rl.GetMousePosition(), data.Cam);
        (double worldX, double worldY) = inv * (worldPosVec.X, worldPosVec.Y);
        (worldPosVec.X, worldPosVec.Y) = ((float)worldX, (float)worldY);
        bool dragging = UpdateCircles(worldPosVec, data);

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

        return dragging || TopLeft.Hovered || TopRight.Hovered || BotLeft.Hovered || BotRight.Hovered;
    }

    private bool UpdateCircles(Vector2 mouseWorldPos, OverlayData data)
    {
        TopLeft.Update(data, true, mouseWorldPos);
        if (TopLeft.Dragging)
        {
            BotLeft.X = TopLeft.X;
            TopRight.Y = TopLeft.Y;
        }
        bool dragging = TopLeft.Dragging;

        TopRight.Update(data, !dragging, mouseWorldPos);
        if (!dragging && TopRight.Dragging)
        {
            BotRight.X = TopRight.X;
            TopLeft.Y = TopRight.Y;
        }
        dragging |= TopRight.Dragging;

        BotLeft.Update(data, !dragging, mouseWorldPos);
        if (!dragging && BotLeft.Dragging)
        {
            TopLeft.X = BotLeft.X;
            BotRight.Y = BotLeft.Y;
        }
        dragging |= BotLeft.Dragging;

        BotRight.Update(data, !dragging, mouseWorldPos);
        if (!dragging && BotRight.Dragging)
        {
            TopRight.X = BotRight.X;
            BotLeft.Y = BotRight.Y;
        }
        dragging |= BotRight.Dragging;
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