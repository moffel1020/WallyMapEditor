using System;
using System.Collections.Generic;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class AssetOverlay(AbstractAsset asset) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(0, 0);
    public DragCircle TopRight { get; set; } = new(0, 0);
    public DragCircle BotLeft { get; set; } = new(0, 0);
    public DragCircle BotRight { get; set; } = new(0, 0);
    public DragCircle LeftEdge { get; set; } = new(0, 0);
    public DragCircle RightEdge { get; set; } = new(0, 0);
    public DragCircle TopEdge { get; set; } = new(0, 0);
    public DragCircle BottomEdge { get; set; } = new(0, 0);

    public DragBox MoveRect { get; set; } = new(0, 0, 0, 0);
    public RotatePoint RotatePoint { get; set; } = new(0, 0);

    public void Draw(OverlayData data)
    {
        foreach (DragCircle circle in GetCircles())
        {
            circle.Color = data.OverlayConfig.ColorAssetBox;
            circle.UsingColor = data.OverlayConfig.UsingColorAssetBox;
        }
        MoveRect.Color = data.OverlayConfig.ColorAssetBox;
        MoveRect.UsingColor = data.OverlayConfig.UsingColorAssetBox;
        RotatePoint.LineColor = data.OverlayConfig.ColorAssetRotationLine;

        if (!RotatePoint.Active)
        {
            foreach (DragCircle circle in GetCircles())
                circle.Draw(data);
        }

        MoveRect.Draw(data);
        RotatePoint.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        if (asset.AssetName is null) throw new ArgumentException("AssetOverlay used on asset without AssetName");

        foreach (DragCircle circle in GetCircles())
            circle.Radius = data.OverlayConfig.RadiusAssetCorner;

        WmsTransform trans = FullTransform(asset, data.Context);
        WmsTransform inv = WmsTransform.CreateInverse(trans);

        double w = asset.W!.Value;
        double h = asset.H!.Value;

        (TopLeft.X, TopLeft.Y) = trans * (0, 0);
        (TopRight.X, TopRight.Y) = trans * (w, 0);
        (BotLeft.X, BotLeft.Y) = trans * (0, h);
        (BotRight.X, BotRight.Y) = trans * (w, h);
        (LeftEdge.X, LeftEdge.Y) = trans * (0, h / 2);
        (RightEdge.X, RightEdge.Y) = trans * (w, h / 2);
        (TopEdge.X, TopEdge.Y) = trans * (w / 2, 0);
        (BottomEdge.X, BottomEdge.Y) = trans * (w / 2, h);

        MoveRect.Transform = trans;
        (MoveRect.X, MoveRect.Y) = (0, 0);
        (MoveRect.W, MoveRect.H) = (w, h);

        (RotatePoint.X, RotatePoint.Y) = (trans.TranslateX, trans.TranslateY);
        RotatePoint.Update(data, true, asset.Rotation * Math.PI / 180);
        if (RotatePoint.Active)
        {
            cmd.Add(new PropChangeCommand<double>(val => asset.Rotation = val, asset.Rotation, RotatePoint.Rotation * 180 / Math.PI));
        }

        bool dragging = UpdateCircles(data, trans, inv);
        // POSSIBLE OPTIMIZATION: at the end of UpdateCircles we transform by trans, and here we undo that
        // so we can save two transforms here
        TransfromDragCircles(inv);
        (double newW, double newH) = (TopRight.X - TopLeft.X, BotLeft.Y - TopLeft.Y);
        (double newX, double newY) = asset.Transform * TopLeft.Position;
        (double offX, double offY) = TopLeft.Position;
        TransfromDragCircles(trans);

        if (dragging)
        {
            // update MoveRect for drawing
            MoveRect.Transform = trans * WmsTransform.CreateTranslate(offX, offY);
            (MoveRect.X, MoveRect.Y) = (0, 0);
            (MoveRect.W, MoveRect.H) = (newW, newH);

            cmd.Add(new PropChangeCommand<double, double, double, double>(
                (val1, val2, val3, val4) => (asset.X, asset.Y, asset.W, asset.H) = (val1, val2, val3, val4),
                asset.X, asset.Y, asset.W.Value, asset.H.Value,
                newX, newY, newW, newH
            ));
        }

        MoveRect.Update(data, !dragging && !RotatePoint.Active);

        if (MoveRect.Dragging)
        {
            // update circles for drawing
            // TODO: can this somehow be unified with the update above?
            WmsTransform newTransform = trans * WmsTransform.CreateTranslate(MoveRect.X, MoveRect.Y);
            (TopLeft.X, TopLeft.Y) = newTransform * (0, 0);
            (TopRight.X, TopRight.Y) = newTransform * (w, 0);
            (BotLeft.X, BotLeft.Y) = newTransform * (0, h);
            (BotRight.X, BotRight.Y) = newTransform * (w, h);
            (LeftEdge.X, LeftEdge.Y) = newTransform * (0, h / 2);
            (RightEdge.X, RightEdge.Y) = newTransform * (w, h / 2);
            (TopEdge.X, TopEdge.Y) = newTransform * (w / 2, 0);
            (BottomEdge.X, BottomEdge.Y) = newTransform * (w / 2, h);

            (double newAssetX, double newAssetY) = asset.Transform * (MoveRect.X, MoveRect.Y);
            cmd.Add(new PropChangeCommand<double, double>(
                (val1, val2) => (asset.X, asset.Y) = (val1, val2),
                asset.X, asset.Y,
                newAssetX, newAssetY
            ));
        }

        return dragging || MoveRect.Dragging || RotatePoint.Active;
    }

    private bool UpdateCircles(OverlayData data, WmsTransform trans, WmsTransform invTrans)
    {
        double x = 0;
        double y = 0;
        double w = asset.W!.Value;
        double h = asset.H!.Value;

        double oldX = x;
        double oldY = y;
        double oldW = w;
        double oldH = h;

        double ratioH = oldH / oldW;
        double ratioW = oldW / oldH;

        bool mirrorDrag = Rl.IsKeyDown(KeyboardKey.LeftAlt);
        bool scaleDrag = Rl.IsKeyDown(KeyboardKey.LeftShift);

        bool dragging = false;
        foreach (DragCircle circle in GetCircles())
        {
            circle.Update(data, !dragging && !RotatePoint.Active);
            dragging |= circle.Dragging;
        }

        TransfromDragCircles(invTrans);

        if (TopLeft.Dragging)
        {
            x = TopLeft.X;
            y = TopLeft.Y;
            w = TopRight.X - TopLeft.X;
            h = BotLeft.Y - TopLeft.Y;

            if (scaleDrag)
            {
                if (TopLeft.X - oldX < TopLeft.Y - oldY)
                {
                    w = h * ratioW;
                    x = oldX - w + oldW;
                }
                else
                {
                    h = w * ratioH;
                    y = oldY - h + oldH;
                }
            }
        }
        else if (TopRight.Dragging)
        {
            // x is unchanged
            y = TopRight.Y;
            w = TopRight.X - TopLeft.X;
            h = BotRight.Y - TopRight.Y;

            if (scaleDrag)
            {
                if (oldX + oldW - TopRight.X < TopRight.Y - oldY)
                {
                    w = h * ratioW;
                }
                else
                {
                    h = w * ratioH;
                    y = oldY - h + oldH;
                }
            }
        }
        else if (BotLeft.Dragging)
        {
            x = BotLeft.X;
            // y in unchanged
            w = BotRight.X - BotLeft.X;
            h = BotLeft.Y - TopLeft.Y;

            if (scaleDrag)
            {
                if (BotLeft.X - oldX < oldY + oldH - BotLeft.Y)
                {
                    w = h * ratioW;
                    x = oldX - w + oldW;
                }
                else
                {
                    h = w * ratioH;
                }
            }
        }
        else if (BotRight.Dragging)
        {
            // x is unchanged
            // y is unchanged
            w = BotRight.X - BotLeft.X;
            h = BotRight.Y - TopRight.Y;

            if (scaleDrag)
            {

                if (oldX + oldW - BotRight.X < oldY + oldH - BotRight.Y)
                {
                    w = h * ratioW;
                }
                else
                {
                    h = w * ratioH;
                }
            }
        }
        else if (LeftEdge.Dragging)
        {
            x = LeftEdge.X;
            // y is unchanged
            w = RightEdge.X - LeftEdge.X;
            // h is unchanged

            if (scaleDrag)
            {
                h = w * ratioH;
            }
        }
        else if (RightEdge.Dragging)
        {
            // x is unchanged
            // y is unchanged
            w = RightEdge.X - LeftEdge.X;
            // h is unchanged

            if (scaleDrag)
            {
                h = w * ratioH;
            }
        }
        else if (TopEdge.Dragging)
        {
            // x is unchanged
            y = TopEdge.Y;
            // w is unchanged
            h = BottomEdge.Y - TopEdge.Y;

            if (scaleDrag)
            {
                w = h * ratioW;
            }
        }
        else if (BottomEdge.Dragging)
        {
            // x is unchanged
            // y is unchanged
            // w is unchanged
            h = BottomEdge.Y - TopEdge.Y;

            if (scaleDrag)
            {
                w = h * ratioW;
            }
        }

        if (mirrorDrag)
        {
            double offX = x - oldX;
            double offY = y - oldY;
            double offW = w - oldW;
            double offH = h - oldH;
            x -= offW;
            y -= offH;
            w += offW - offX;
            h += offH - offY;
        }

        if (dragging)
        {
            (TopLeft.X, TopLeft.Y) = (x, y);
            (TopRight.X, TopRight.Y) = (x + w, y);
            (BotLeft.X, BotLeft.Y) = (x, y + h);
            (BotRight.X, BotRight.Y) = (x + w, y + h);
            (LeftEdge.X, LeftEdge.Y) = (x, y + h / 2);
            (RightEdge.X, RightEdge.Y) = (x + w, y + h / 2);
            (TopEdge.X, TopEdge.Y) = (x + w / 2, y);
            (BottomEdge.X, BottomEdge.Y) = (x + w / 2, y + h);
        }

        TransfromDragCircles(trans);

        return dragging;
    }

    public static WmsTransform FullTransform(AbstractAsset? a, RenderContext context) => a switch
    {
        MovingPlatform m => GetMovingPlatformTransform(m, context),
        AbstractAsset => FullTransform(a.Parent, context) * a.Transform,
        null => WmsTransform.IDENTITY
    };

    private static WmsTransform GetMovingPlatformTransform(MovingPlatform m, RenderContext context)
    {
        return context.PlatIDMovingPlatformTransform.GetValueOrDefault(m.PlatID, WmsTransform.IDENTITY);
    }

    private void TransfromDragCircles(WmsTransform trans)
    {
        foreach (DragCircle circle in GetCircles())
        {
            (circle.X, circle.Y) = trans * circle.Position;
        }
    }

    private IEnumerable<DragCircle> GetCircles()
    {
        yield return TopLeft;
        yield return TopRight;
        yield return BotLeft;
        yield return BotRight;
        yield return LeftEdge;
        yield return RightEdge;
        yield return TopEdge;
        yield return BottomEdge;
    }
}