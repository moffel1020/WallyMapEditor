namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowSpawnBotBoundsProps(SpawnBotBounds sb, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragFloatHistory("X##botbounds", sb.X, val => sb.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y##botbounds", sb.Y, val => sb.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("W##botbounds", sb.W, val => sb.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory("H##botbounds", sb.H, val => sb.H = val, cmd, minValue: 1);
        return propChanged;
    }
}