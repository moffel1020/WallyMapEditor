namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowCameraBoundsProps(CameraBounds cb, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragFloatHistory("X##cambounds", cb.X, val => cb.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y##cambounds", cb.Y, val => cb.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("W##cambounds", cb.W, val => cb.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory("H##cambounds", cb.H, val => cb.H = val, cmd, minValue: 1);
        return propChanged;
    }
}