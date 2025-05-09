using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

partial class PropertiesWindow
{
    public static bool ShowLevelAnimProps(LevelAnim la, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;
        LevelDesc ld = level.Level.Desc;

        RemoveButton(la, level);
        ImGui.Separator();

        bool propChanged = false;
        string name = la.InstanceName;
        ImGui.InputText("InstanceName", ref name, 64);
        if (name != la.InstanceName)
        {
            cmd.Add(new PropChangeCommand<string>(val => la.InstanceName = val, la.InstanceName, name));
            propChanged = true;
        }
        ImGui.Text("AssetName: " + la.AssetName);
        propChanged |= ImGuiExt.DragDoubleHistory("X", la.X, val => la.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", la.Y, val => la.Y = val, cmd);

        return propChanged;
    }
}