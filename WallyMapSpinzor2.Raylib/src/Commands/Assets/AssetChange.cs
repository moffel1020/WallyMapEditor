namespace WallyMapSpinzor2.Raylib;

public class AssetChange(AbstractAsset asset, double x, double y, double scaleX, double scaleY, double rotation) : ICommand
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double ScaleX { get; set; } = scaleX;
    public double ScaleY { get; set; } = scaleY;
    public double Rotation { get; set; } = rotation;
    private readonly AbstractAsset _asset = asset;

    public bool AllowMerge { get; set; } = true;

    public void Execute()
    {
        _asset.X += X;
        _asset.Y += Y;
        _asset.ScaleX += ScaleX;
        _asset.ScaleY += ScaleY;
        _asset.Rotation += Rotation;
    }

    public void Undo()
    {
        _asset.X -= X;
        _asset.Y -= Y;
        _asset.ScaleX -= ScaleX;
        _asset.ScaleY -= ScaleY;
        _asset.Rotation -= Rotation;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is AssetChange other && _asset == other._asset)
        {
            X += other.X;
            Y += other.Y;
            ScaleX += other.ScaleX;
            ScaleY += other.ScaleY;
            Rotation += other.Rotation;
            return true;
        }
        return false;
    }
}