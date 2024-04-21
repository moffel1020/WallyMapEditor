using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow 
{
    public bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd)
    {
        if (a.AssetName is not null)
            ImGui.Text(a.AssetName);
        
        // TODO: change image asset here somehow

        double dx = ImGuiExt.DragFloat("x", (float)a.X) - (float)a.X;
        double dy = ImGuiExt.DragFloat("y", (float)a.Y) - (float)a.Y;

        ImGui.Separator();
        double scaleX = ImGuiExt.DragFloat("scaleX", (float)a.ScaleX, speed: 0.01) - (float)a.ScaleX;
        double scaleY = ImGuiExt.DragFloat("scaleY", (float)a.ScaleY, speed: 0.01) - (float)a.ScaleY;

        ImGui.Separator();
        double rot = ImGuiExt.DragFloat("rotation", (float)a.Rotation) % 360 - (float)a.Rotation;

        if (dx != 0 || dy != 0 || scaleX != 0 || scaleY != 0 || rot != 0)
        {
            _propChanged = true;
            cmd.Add(new AssetChange(a, dx, dy, scaleX, scaleY, rot));
        }

        return true;
    }
}