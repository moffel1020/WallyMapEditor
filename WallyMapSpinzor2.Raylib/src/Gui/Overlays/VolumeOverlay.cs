namespace WallyMapSpinzor2.Raylib;

public class VolumeOverlay(AbstractVolume volume) : IOverlay
{
    public DragCircle TopLeft { get; set; } = new(volume.X, volume.Y);
    public DragCircle TopRight { get; set; } = new(volume.X + volume.W, volume.Y);
    public DragCircle BotLeft { get; set; } = new(volume.X, volume.Y + volume.H);
    public DragCircle BotRight { get; set; } = new(volume.X + volume.W, volume.Y + volume.H);

    public DragBox MoveRect { get; set; } = new(volume.X, volume.Y, volume.W, volume.H);

    public void Draw(OverlayData data)
    {
        TopLeft.Draw(data);
        TopRight.Draw(data);
        BotLeft.Draw(data);
        BotRight.Draw(data);
        MoveRect.Draw(data);
    }

    public bool Update(OverlayData data, CommandHistory cmd)
    {
        (TopLeft.X, TopLeft.Y) = (volume.X, volume.Y);
        (TopRight.X, TopRight.Y) = (volume.X + volume.W, volume.Y);
        (BotLeft.X, BotLeft.Y) = (volume.X, volume.Y + volume.H);
        (BotRight.X, BotRight.Y) = (volume.X + volume.W, volume.Y + volume.H);
        (MoveRect.X, MoveRect.Y, MoveRect.W, MoveRect.H) = (volume.X, volume.Y, volume.W, volume.H);

        TopLeft.Update(data, true);
        TopLeft.X = (int)TopLeft.X;
        TopLeft.Y = (int)TopLeft.Y;
        if (TopLeft.Dragging)
        {
            BotLeft.X = TopLeft.X;
            TopRight.Y = TopLeft.Y;
        }
        bool dragging = TopLeft.Dragging;

        TopRight.Update(data, !dragging);
        TopRight.X = (int)TopRight.X;
        TopRight.Y = (int)TopRight.Y;
        if (!dragging && TopRight.Dragging)
        {
            BotRight.X = TopRight.X;
            TopLeft.Y = TopRight.Y;
        }
        dragging |= TopRight.Dragging;

        BotLeft.Update(data, !dragging);
        BotLeft.X = (int)BotLeft.X;
        BotLeft.Y = (int)BotLeft.Y;
        if (!dragging && BotLeft.Dragging)
        {
            TopLeft.X = BotLeft.X;
            BotRight.Y = BotLeft.Y;
        }
        dragging |= BotLeft.Dragging;

        BotRight.Update(data, !dragging);
        BotRight.X = (int)BotRight.X;
        BotRight.Y = (int)BotRight.Y;
        if (!dragging && BotRight.Dragging)
        {
            TopRight.X = BotRight.X;
            BotLeft.Y = BotRight.Y;
        }
        dragging |= BotRight.Dragging;

        if (dragging)
        {
            cmd.Add(new PropChangeCommand<(int, int, int, int)>(
                val => (volume.X, volume.Y, volume.W, volume.H) = val,
                (volume.X, volume.Y, volume.W, volume.H),
                ((int)TopLeft.X, (int)TopLeft.Y, (int)(TopRight.X - TopLeft.X), (int)(BotLeft.Y - TopLeft.Y))));
        }

        bool resizing = dragging || TopLeft.Hovered || TopRight.Hovered || BotLeft.Hovered || BotRight.Hovered;
        MoveRect.Update(data, !resizing);

        if (MoveRect.Dragging)
        {
            cmd.Add(new PropChangeCommand<(int, int)>(
                val => (volume.X, volume.Y) = val,
                (volume.X, volume.Y),
                ((int)MoveRect.X, (int)MoveRect.Y)));
        }

        return resizing || MoveRect.Dragging || MoveRect.Hovered;
    }
}