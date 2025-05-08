using System.Linq;
using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowMovingPlatformProps(MovingPlatform mp, CommandHistory cmd, PropertiesWindowData data)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.InputTextHistory("PlatID", mp.PlatID, val => mp.PlatID = val, cmd, 32);

        ImGui.Separator();
        if (data.Level is not null)
            RemoveButton(mp, data.Level.Desc, cmd);
        ImGui.Separator();

        if (data.Level is not null && ImGui.TreeNode("Connected dynamics"))
        {
            ShowConnectedDynamics(data.Level.Desc, mp, data.Selection);
            ImGui.TreePop();
        }
        ImGui.Separator();

        propChanged |= ImGuiExt.DragDoubleHistory("X##mp", mp.X, val => mp.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y##mp", mp.Y, val => mp.Y = val, cmd);
        if (ImGui.CollapsingHeader("Animation"))
            propChanged |= ShowAnimationProps(mp.Animation, cmd);
        ImGui.Separator();
        if (mp.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", mp.Assets, val => mp.Assets = val,
            () => CreateNewMovingPlatformChild(mp),
            index =>
            {
                bool changed = false;
                if (index >= mp.Assets.Length) return false;
                AbstractAsset child = mp.Assets[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##mpchild{index}") && data.Selection is not null)
                    data.Selection.Object = child;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }
        return propChanged;
    }

    private static void ShowConnectedDynamics(LevelDesc desc, MovingPlatform mp, SelectionContext? selection)
    {
        foreach (DynamicCollision dc in desc.DynamicCollisions.Where(d => mp.PlatID == d.PlatID).SkipLast(1))
        {
            if (ImGui.Button($"Collision ({dc.X:0.###}, {dc.Y:0.###})##{dc.GetHashCode()}") && selection is not null)
                selection.Object = dc;
            ImGui.SameLine();
            ImGui.TextDisabled("(disabled)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Only one DynamicCollision can be animated by a movingplatform. idk ask bmg");
        }

        if (desc.DynamicCollisions.Where(d => mp.PlatID == d.PlatID).LastOrDefault() is DynamicCollision lastDc)
            if (ImGui.Button($"Collision ({lastDc.X:0.###}, {lastDc.Y:0.###})##{lastDc.GetHashCode()}") && selection is not null)
                selection.Object = lastDc;

        foreach (DynamicRespawn dr in desc.DynamicRespawns.Where(d => d.PlatID == mp.PlatID))
            if (ImGui.Button($"Respawn ({dr.X:0.###}, {dr.Y:0.###})##{dr.GetHashCode()}") && selection is not null)
                selection.Object = dr;

        foreach (DynamicItemSpawn di in desc.DynamicItemSpawns.Where(d => d.PlatID == mp.PlatID))
            if (ImGui.Button($"ItemSpawn ({di.X:0.###}, {di.Y:0.###})##{di.GetHashCode()}") && selection is not null)
                selection.Object = di;

        foreach (DynamicNavNode dn in desc.DynamicNavNodes.Where(d => d.PlatID == mp.PlatID))
            if (ImGui.Button($"NavNode ({dn.X:0.###}, {dn.Y:0.###})##{dn.GetHashCode()}") && selection is not null)
                selection.Object = dn;
    }

    private static Maybe<AbstractAsset> CreateNewMovingPlatformChild(MovingPlatform parent)
    {
        Maybe<AbstractAsset> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##moving_platform");

        if (ImGui.BeginPopup("AddChild##moving_platform"))
        {
            result = AddObjectPopup.AddAssetMenu(0, 0);
            result.DoIfSome(a => a.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }
}