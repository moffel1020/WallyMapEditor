namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public bool ShowItemSpawnProps(AbstractItemSpawn i, CommandHistory cmd)
    {
        double x = ImGuiExt.DragFloat($"x##props{i.GetHashCode}", (float)i.X) - (float)i.X;
        double y = ImGuiExt.DragFloat($"y##props{i.GetHashCode}", (float)i.Y) - (float)i.Y;
        double w = ImGuiExt.DragFloat($"w##props{i.GetHashCode}", (float)i.W, minValue: 1) - (float)i.W;
        double h = ImGuiExt.DragFloat($"h##props{i.GetHashCode}", (float)i.H, minValue: 1) - (float)i.H;

        if (x != 0 || y != 0)
        {
            _propChanged = true;
            cmd.Add(new ItemSpawnMove(i, x, y));
        }

        if (w != 0 || h != 0)
        {
            _propChanged = true;
            cmd.Add(new ItemSpawnResize(i, w, h));
        }

        return true;
    }
}