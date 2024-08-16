using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowAbstractVolumeProps(AbstractVolume v, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragIntHistory($"X##props{v.GetHashCode()}", v.X, val => v.X = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory($"Y##props{v.GetHashCode()}", v.Y, val => v.Y = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory($"W##props{v.GetHashCode()}", v.W, val => v.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragIntHistory($"H##props{v.GetHashCode()}", v.H, val => v.H = val, cmd, minValue: 1);
        int newTeam = int.Parse(ImGuiExt.StringCombo($"Team##props{v.GetHashCode()}", v.Team.ToString(), ["0", "1", "2", "3", "4", "5"]));
        if (v.Team != newTeam)
        {
            cmd.Add(new PropChangeCommand<int>(val => v.Team = val, v.Team, newTeam));
            propChanged = true;
        }
        return propChanged;
    }
}