namespace WallyMapSpinzor2.Raylib;

public class ItemSpawnMove(AbstractItemSpawn item, double x, double y): ICommand
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    private readonly AbstractItemSpawn _itemSpawn = item;
    
    public bool AllowMerge { get; set; } = true;

    public void Execute()
    {
        _itemSpawn.X += X;
        _itemSpawn.Y += Y;
    }

    public void Undo()
    {
        _itemSpawn.X -= X;
        _itemSpawn.Y -= Y;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is ItemSpawnMove other && _itemSpawn == other._itemSpawn)
        {
            X += other.X;
            Y += other.Y;
            return true;
        }

        return false;
    }
}