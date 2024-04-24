namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public bool ShowItemSpawnProps(AbstractItemSpawn i, CommandHistory cmd)
    {
        _propChanged |= ImGuiExt.DragFloatHistory($"x##props{i.GetHashCode()}", i.X, (val) => i.X = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"y##props{i.GetHashCode()}", i.Y, (val) => i.Y = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"w##props{i.GetHashCode()}", i.W, (val) => i.W = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"h##props{i.GetHashCode()}", i.H, (val) => i.H = val, cmd);
        return true;
    }
}