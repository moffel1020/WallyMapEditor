namespace WallyMapSpinzor2.Raylib;

public class ItemSpawnResize(AbstractItemSpawn item, double w, double h) : ICommand
{
    public double W { get; set; } = w;
    public double H { get; set; } = h;
    private readonly AbstractItemSpawn _itemSpawn =  item;

    public bool AllowMerge { get; set; } = true;

    public void Execute()
    {
        _itemSpawn.W += W;
        _itemSpawn.H += H;
    }

    public void Undo()
    {
        _itemSpawn.W -= W;
        _itemSpawn.H -= H;
    }

    public bool Merge(ICommand cmd)
    {
        if (cmd is ItemSpawnResize other && _itemSpawn == other._itemSpawn)
        {
            W += other.W;
            H += other.H;
            return true;
        }

        return false;
    }
}