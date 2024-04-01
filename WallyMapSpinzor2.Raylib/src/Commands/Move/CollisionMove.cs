namespace WallyMapSpinzor2.Raylib;

public class CollisionMove(AbstractCollision col, double x1, double x2, double y1, double y2) : ICommand
{
    public double X1 { get; set; } = x1;
    public double X2 { get; set; } = x2;
    public double Y1 { get; set; } = y1;
    public double Y2 { get; set; } = y2;
    private readonly AbstractCollision _collision = col;

    public bool AllowMerge { get; set; } = true;

    public void Execute()
    {
        _collision.X1 += X1;
        _collision.X2 += X2;
        _collision.Y1 += Y1;
        _collision.Y2 += Y2;
    }

    public void Undo()
    {
        _collision.X1 -= X1;
        _collision.X2 -= X2;
        _collision.Y1 -= Y1;
        _collision.Y2 -= Y2;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is CollisionMove other && _collision == other._collision)
        {
            X1 += other.X1;
            X2 += other.X2;
            Y1 += other.Y1;
            Y2 += other.Y2;
            return true;
        }

        return false;
    }
}