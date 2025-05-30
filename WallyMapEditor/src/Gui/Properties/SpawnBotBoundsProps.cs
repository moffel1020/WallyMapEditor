using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowSpawnBotBoundsProps(SpawnBotBounds sb, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;
        propChanged |= ImGuiExt.DragDoubleHistory("X##botbounds", sb.X, val => sb.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y##botbounds", sb.Y, val => sb.Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("W##botbounds", sb.W, val => sb.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragDoubleHistory("H##botbounds", sb.H, val => sb.H = val, cmd, minValue: 1);
        return propChanged;
    }
}