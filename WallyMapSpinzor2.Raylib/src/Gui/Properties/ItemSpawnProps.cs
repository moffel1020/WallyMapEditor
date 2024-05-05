namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowItemSpawnProps(AbstractItemSpawn i, CommandHistory cmd)
    {
        bool propChanged = ImGuiExt.DragFloatHistory($"x##props{i.GetHashCode()}", i.X, val => i.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"y##props{i.GetHashCode()}", i.Y, val => i.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"w##props{i.GetHashCode()}", i.W, val => i.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory($"h##props{i.GetHashCode()}", i.H, val => i.H = val, cmd, minValue: 1);
        return propChanged;
    }
}