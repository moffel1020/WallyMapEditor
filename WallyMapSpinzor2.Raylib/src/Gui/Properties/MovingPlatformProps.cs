using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowMovingPlatformProps(MovingPlatform mp, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("PlatID: " + mp.PlatID);
        propChanged |= ImGuiExt.DragFloatHistory("x##mp", mp.X, val => mp.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("y##mp", mp.Y, val => mp.Y = val, cmd);

        if (ImGui.CollapsingHeader("Animation"))
            propChanged |= ShowAnimationProps(mp.Animation, cmd);

        ImGui.Separator();
        if (mp.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            foreach (AbstractAsset child in mp.Assets)
            {
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}##{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd);
                    ImGui.TreePop();
                }
            }
        }
        return propChanged;
    }
}