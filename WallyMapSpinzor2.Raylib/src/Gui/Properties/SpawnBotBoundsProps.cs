namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowSpawnBotBoundsProps(SpawnBotBounds sb, CommandHistory cmd)
    {
        bool propChanged = ImGuiExt.DragFloatHistory("x##botbounds", sb.X, val => sb.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("y##botbounds", sb.Y, val => sb.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("w##botbounds", sb.W, val => sb.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory("h##botbounds", sb.H, val => sb.H = val, cmd, minValue: 1);
        return propChanged;
    }
}