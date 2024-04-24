namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public bool ShowAbstractCollisionProps(AbstractCollision ac, CommandHistory cmd)
    {
        _propChanged |= ImGuiExt.DragFloatHistory($"x1##props{ac.GetHashCode()}", ac.X1, (val) => ac.X1 = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"y1##props{ac.GetHashCode()}", ac.Y1, (val) => ac.Y1 = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"x2##props{ac.GetHashCode()}", ac.X2, (val) => ac.X2 = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"y2##props{ac.GetHashCode()}", ac.Y2, (val) => ac.Y2 = val, cmd);

        return true;
    }
}