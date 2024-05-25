using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowLevelAnimProps(LevelAnim la, CommandHistory cmd)
    {
        bool propChanged = false;

        string name = la.InstanceName;
        ImGui.InputText("InstanceName", ref name, 64);
        la.InstanceName = name;

        ImGui.Text("AssetName: " + la.AssetName);

        propChanged |= ImGuiExt.DragFloatHistory("X", la.X, val => la.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", la.Y, val => la.Y = val, cmd);

        return propChanged;
    }
}