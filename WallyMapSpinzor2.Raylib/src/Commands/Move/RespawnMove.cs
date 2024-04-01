namespace WallyMapSpinzor2.Raylib;

public class RespawnMove(Respawn res, double x, double y): ICommand
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    private readonly Respawn _respawn = res;

    public bool AllowMerge { get; set; } = true;

    public void Execute()
    {
        _respawn.X += X;
        _respawn.Y += Y;
    }

    public void Undo()
    {
        _respawn.X -= X;
        _respawn.Y -= Y;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is RespawnMove other && _respawn == other._respawn)
        {
            X += other.X;
            Y += other.Y;
            return true;
        }

        return false;
    }
}   