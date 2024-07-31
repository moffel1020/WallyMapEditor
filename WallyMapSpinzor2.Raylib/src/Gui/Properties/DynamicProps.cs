using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, CommandHistory cmd, PropertiesWindowData data)
        where T : IDeserializable, ISerializable, IDrawable
    {
        bool propChanged = false;
        ImGui.Text("PlatID: " + ad.PlatID);
        propChanged |= ImGuiExt.DragFloatHistory("X", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", ad.Y, val => ad.Y = val, cmd);

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