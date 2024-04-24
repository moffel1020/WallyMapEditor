using System.Runtime.Intrinsics.X86;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow 
{
    public bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd)
    {
        if (a.AssetName is not null)
            ImGui.Text(a.AssetName);
        
        // TODO: change image asset here somehow

        _propChanged |= ImGuiExt.DragFloatHistory("x", a.X, (val) => a.X = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory("y", a.Y, (val) => a.Y = val, cmd);
        ImGui.Separator();
        _propChanged |= ImGuiExt.DragFloatHistory("scaleX", a.ScaleX, (val) => a.ScaleX = val, cmd, speed: 0.01);
        _propChanged |= ImGuiExt.DragFloatHistory("scaleY", a.ScaleY, (val) => a.ScaleY = val, cmd, speed: 0.01);
        ImGui.Separator();
        _propChanged |= ImGuiExt.DragFloatHistory("rotation", a.Rotation, (val) => a.Rotation = val, cmd, speed: 0.01, minValue: 0, maxValue: 360);

        return true;
    }
}