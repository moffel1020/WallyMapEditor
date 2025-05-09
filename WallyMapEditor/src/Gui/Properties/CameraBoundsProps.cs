using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowCameraBoundsProps(CameraBounds cb, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;
        propChanged |= ImGuiExt.DragDoubleHistory("X##cambounds", cb.X, val => cb.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y##cambounds", cb.Y, val => cb.Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("W##cambounds", cb.W, val => cb.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragDoubleHistory("H##cambounds", cb.H, val => cb.H = val, cmd, minValue: 1);
        return propChanged;
    }
}