using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, CommandHistory cmd, PropertiesWindowData data)
        where T : IDeserializable, ISerializable, IDrawable
    {
        bool propChanged = false;

        string[] validPlatIds = data.Level?.Desc.Assets.OfType<MovingPlatform>().Select(mp => mp.PlatID).ToArray() ?? [];
        if (validPlatIds.Length > 0)
        {
            propChanged |= ImGuiExt.GenericStringComboHistory("PlatID", ad.PlatID, val => ad.PlatID = val, val => val, val => val, validPlatIds, cmd);
            if (ImGui.Button("Select MovingPlatform"))
                data.Selection.Object = data.Level!.Desc.Assets.OfType<MovingPlatform>().Where(mp => mp.PlatID == ad.PlatID).Single();
        }
        else
        {
            ImGui.Text("PlatID: " + ad.PlatID);
        }

        propChanged |= ImGuiExt.DragDoubleHistory("X", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", ad.Y, val => ad.Y = val, cmd);

        if (ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", ad.Children, val => ad.Children = val,
            () => CreateDynamicChild(ad),
            (int index) =>
            {
                bool changed = false;
                T child = ad.Children[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##dyncol{child.GetHashCode()}")) data.Selection.Object = child;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }

        return propChanged;
    }

    private static Maybe<T> CreateDynamicChild<T>(AbstractDynamic<T> parent)
        where T : IDeserializable, ISerializable, IDrawable => parent switch
        {
            DynamicCollision col => CreateCollisionChild(col).Cast<T>(),
            DynamicItemSpawn item => CreateItemSpawnChild(item).Cast<T>(),
            DynamicRespawn res => CreateRespawnChild(res).Cast<T>(),
            _ => Maybe<T>.None
        };

    private static Maybe<AbstractCollision> CreateCollisionChild(DynamicCollision parent)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddCollisionMenu(new(0, 0));
            result.DoIfSome(col => col.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<AbstractItemSpawn> CreateItemSpawnChild(DynamicItemSpawn parent)
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddItemSpawnMenu(new(0, 0));
            result.DoIfSome(col => col.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<Respawn> CreateRespawnChild(DynamicRespawn parent)
    {
        Maybe<Respawn> result = new();
        if (ImGui.Button("Add new respawn"))
        {
            result = DefaultRespawn(new(0, 0));
            result.Value.Parent = parent;
        }
        return result;
    }
}