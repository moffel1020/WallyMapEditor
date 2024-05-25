using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd)
    {
        if (a.AssetName is not null)
            ImGui.Text("AssetName: " + a.AssetName);

        // TODO: change image asset here somehow

        bool propChanged = ImGuiExt.DragFloatHistory("X", a.X, val => a.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", a.Y, val => a.Y = val, cmd);
        ImGui.Separator();
        propChanged |= ImGuiExt.DragFloatHistory("ScaleX", a.ScaleX, val => a.ScaleX = val, cmd, speed: 0.01);
        propChanged |= ImGuiExt.DragFloatHistory("ScaleY", a.ScaleY, val => a.ScaleY = val, cmd, speed: 0.01);
        ImGui.Separator();
        propChanged |= ImGuiExt.DragFloatHistory("Rotation", a.Rotation, val => a.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1);

        return propChanged;
    }
}