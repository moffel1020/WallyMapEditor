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
                T child = ad.Children[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }
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
            if (ImGui.BeginMenu("Normal Collision"))
            {
                if (ImGui.MenuItem(nameof(HardCollision))) result = DefaultCollision<HardCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(SoftCollision))) result = DefaultCollision<SoftCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(NoSlideCollision))) result = DefaultCollision<NoSlideCollision>(new(0, 0));
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Bouncy Collision"))
            {
                if (ImGui.MenuItem(nameof(BouncyHardCollision))) result = DefaultCollision<BouncyHardCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(BouncySoftCollision))) result = DefaultCollision<BouncySoftCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(BouncyNoSlideCollision))) result = DefaultCollision<BouncyNoSlideCollision>(new(0, 0));
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Special Collision"))
            {
                if (ImGui.MenuItem(nameof(StickyCollision))) result = DefaultCollision<StickyCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(ItemIgnoreCollision))) result = DefaultCollision<ItemIgnoreCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(TriggerCollision))) result = DefaultCollision<TriggerCollision>(new(0, 0));
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Gamemode collision"))
            {
                if (ImGui.MenuItem(nameof(GameModeHardCollision))) result = DefaultCollision<GameModeHardCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(PressurePlateCollision))) result = DefaultCollision<PressurePlateCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(SoftPressurePlateCollision))) result = DefaultCollision<SoftPressurePlateCollision>(new(0, 0));
                if (ImGui.MenuItem(nameof(LavaCollision))) result = DefaultCollision<LavaCollision>(new(0, 0));
                ImGui.EndMenu();
            }
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
            if (ImGui.MenuItem(nameof(ItemSpawn))) result = DefaultItemSpawn<ItemSpawn>(new(0, 0));
            if (ImGui.MenuItem(nameof(ItemInitSpawn))) result = DefaultItemSpawn<ItemInitSpawn>(new(0, 0));
            if (ImGui.MenuItem(nameof(TeamItemInitSpawn))) result = DefaultItemSpawn<TeamItemInitSpawn>(new(0, 0));
            if (ImGui.MenuItem(nameof(ItemSet))) result = DefaultItemSpawn<ItemSet>(new(0, 0));
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