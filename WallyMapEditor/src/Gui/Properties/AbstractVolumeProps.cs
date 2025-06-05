using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowAbstractVolumeProps(AbstractVolume v, EditorLevel level, PropertiesWindowData data)
    {
        CommandHistory cmd = level.CommandHistory;
        LevelDesc ld = level.Level.Desc;

        RemoveButton(v, level);
        ImGui.Separator();

        bool propChanged = false;
        propChanged |= WmeUtils.ObjectChangeType(v, ld, cmd, ShowChangeVolumeTypeMenu);
        propChanged |= ImGuiExt.DragIntHistory($"X##props{v.GetHashCode()}", v.X, val => v.X = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory($"Y##props{v.GetHashCode()}", v.Y, val => v.Y = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory($"W##props{v.GetHashCode()}", v.W, val => v.W = val, cmd, minValue: 1);
        propChanged |= ImGuiExt.DragIntHistory($"H##props{v.GetHashCode()}", v.H, val => v.H = val, cmd, minValue: 1);
        uint newTeam = uint.Parse(ImGuiExt.StringCombo($"Team##props{v.GetHashCode()}", v.Team.ToString(), ["0", "1", "2", "3", "4", "5"]));
        if (v.Team != newTeam)
        {
            cmd.Add(new PropChangeCommand<uint>(val => v.Team = val, v.Team, newTeam));
            propChanged = true;
        }
        return propChanged;
    }

    private static Maybe<AbstractVolume> ShowChangeVolumeTypeMenu(AbstractVolume og)
    {
        Maybe<AbstractVolume> result = new();
        if (ImGui.Button("Change Type"))
            ImGui.OpenPopup("ChangeType##volume");

        if (ImGui.BeginPopup("ChangeType##volume"))
        {
            result = AddObjectPopup.AddVolumeMenu(og.X, og.Y).NoneIf(i => i.GetType() == og.GetType());

            result.DoIfSome(volume =>
            {
                volume.H = og.H;
                volume.W = og.W;
                volume.X = og.X;
                volume.Y = og.Y;
                volume.Team = og.Team;
                volume.ID = og.ID;
            });

            ImGui.EndPopup();
        }

        return result;
    }

    public static V DefaultVolume<V>(int x, int y, int w, int h) where V : AbstractVolume, new()
    {
        return new() { X = x, Y = y, W = w, H = h, Team = 1 };
    }
}