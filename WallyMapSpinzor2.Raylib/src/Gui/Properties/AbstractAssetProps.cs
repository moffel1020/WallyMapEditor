using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow 
{
    public static bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd)
    {
        if (a.AssetName is not null)
            ImGui.Text("AssetName" + a.AssetName);

        // TODO: change image asset here somehow
        
        bool propChanged = ImGuiExt.DragFloatHistory("x", a.X, (val) => a.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("y", a.Y, (val) => a.Y = val, cmd);
        ImGui.Separator();
        propChanged |= ImGuiExt.DragFloatHistory("scaleX", a.ScaleX, (val) => a.ScaleX = val, cmd, speed: 0.01);
        propChanged |= ImGuiExt.DragFloatHistory("scaleY", a.ScaleY, (val) => a.ScaleY = val, cmd, speed: 0.01);
        ImGui.Separator();
        propChanged |= ImGuiExt.DragFloatHistory("rotation", a.Rotation, (val) => a.Rotation = val, cmd, speed: 0.1, minValue: 0, maxValue: 360);

        return propChanged;
    }
}