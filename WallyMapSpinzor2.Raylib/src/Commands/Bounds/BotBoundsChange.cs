namespace WallyMapSpinzor2.Raylib;

public class BotBoundsChange(SpawnBotBounds bounds, double x, double y, double w, double h): ICommand
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double W { get; set; } = w;
    public double H { get; set; } = h;
    private readonly SpawnBotBounds _spawnBotBounds = bounds;

    public void Execute()
    {
        _spawnBotBounds.X += X;
        _spawnBotBounds.Y += Y;
        _spawnBotBounds.W += W;
        _spawnBotBounds.H += H;
    }

    public void Undo()
    {
        _spawnBotBounds.X -= X;
        _spawnBotBounds.Y -= Y;
        _spawnBotBounds.W -= W;
        _spawnBotBounds.H -= H;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is BotBoundsChange other && _spawnBotBounds == other._spawnBotBounds)
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