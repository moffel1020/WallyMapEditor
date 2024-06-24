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
            CreateDynamicChild<T>,
            (int index) =>
            {
                bool changed = false;
                T child = ad.Children[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }
                return changed;
            }, cmd);
        }

        return propChanged;
    }

    private static Maybe<T> CreateDynamicChild<T>()
    {
        if (typeof(T) == typeof(AbstractCollision))
            return CreateCollisionChild().Cast<T>();
        if (typeof(T) == typeof(AbstractItemSpawn))
            return CreateItemSpawnChild().Cast<T>();
        if (typeof(T) == typeof(Respawn))
            return CreateRespawnChild().Cast<T>();
        return Maybe<T>.None;
    }

    private static Maybe<AbstractCollision> CreateCollisionChild()
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddCollisionMenu(new(0, 0));
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<AbstractItemSpawn> CreateItemSpawnChild()
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddItemSpawnMenu(new(0, 0));
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<Respawn> CreateRespawnChild()
    {
        Maybe<Respawn> result = new();
        if (ImGui.Button("Add new respawn"))
        {
            result = DefaultRespawn(new(0, 0));
        }
        return result;
    }
}