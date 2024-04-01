namespace WallyMapSpinzor2.Raylib;

public class CameraboundsChange(CameraBounds bounds, double x, double y, double w, double h): ICommand
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double W { get; set; } = w;
    public double H { get; set; } = h;
    private readonly CameraBounds _cameraBounds = bounds;

    public void Execute()
    {
        _cameraBounds.X += X;
        _cameraBounds.Y += Y;
        _cameraBounds.W += W;
        _cameraBounds.H += H;
    }

    public void Undo()
    {
        _cameraBounds.X -= X;
        _cameraBounds.Y -= Y;
        _cameraBounds.W -= W;
        _cameraBounds.H -= H;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is CameraboundsChange other && _cameraBounds == other._cameraBounds)
        {
            X += other.X;
            Y += other.Y;
            W += other.W;
            H += other.H;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}