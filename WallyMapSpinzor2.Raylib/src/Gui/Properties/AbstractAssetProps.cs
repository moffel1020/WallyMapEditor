using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow 
{
    public bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd)
    {
        if (a.AssetName is not null)
            ImGui.Text(a.AssetName);
        
        // TODO: change image asset here somehow

        a.X = ImGuiExt.DragFloat("x", (float)a.X);
        a.Y = ImGuiExt.DragFloat("y", (float)a.Y);

        ImGui.Separator();
        a.ScaleX = ImGuiExt.DragFloat("scaleX", (float)a.ScaleX, speed: 0.01);
        a.ScaleY = ImGuiExt.DragFloat("scaleY", (float)a.ScaleY, speed: 0.01);

        ImGui.Separator();
        a.Rotation = ImGuiExt.DragFloat("rotation", (float)a.Rotation) % 360;

        return true;
    }
}