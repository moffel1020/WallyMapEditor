using System;
using System.Numerics;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public static class AddObjectPopup
{
    public const string NAME = "addobject";
    private static bool _shouldOpen;
    public static void Open() => _shouldOpen = true;

    public static Vector2 NewPos { get; set; }

    public static void Update(Level l, CommandHistory cmd, SelectionContext selection)
    {
        if (_shouldOpen)
        {
            ImGui.OpenPopup(NAME);
            _shouldOpen = false;
        }

        if (!ImGui.BeginPopup(NAME)) return;

        ImGui.SeparatorText("Add new object");

        if (ImGui.BeginMenu("Collisions")) { AddDynamicCollisionMenuHistory(NewPos, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("ItemSpawns")) { AddDynamicItemSpawnMenuHistory(NewPos, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Respawns")) { AddDynamicRespawnMenuHistory(NewPos, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Platforms")) { AddMovingPlatformMenuHistory(NewPos, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("NavNodes")) { AddDynamicNavNodeMenuHistory(NewPos, l, selection, cmd); ImGui.EndMenu(); }

        ImGui.EndPopup();
    }

    public static Maybe<AbstractCollision> AddCollisionMenu(Vector2 start, Vector2 end)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.BeginMenu("Normal Collision"))
        {
            if (ImGui.MenuItem(nameof(HardCollision))) result = PropertiesWindow.DefaultCollision<HardCollision>(start, end);
            if (ImGui.MenuItem(nameof(SoftCollision))) result = PropertiesWindow.DefaultCollision<SoftCollision>(start, end);
            if (ImGui.MenuItem(nameof(NoSlideCollision))) result = PropertiesWindow.DefaultCollision<NoSlideCollision>(start, end);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Bouncy Collision"))
        {
            if (ImGui.MenuItem(nameof(BouncyHardCollision))) result = PropertiesWindow.DefaultCollision<BouncyHardCollision>(start, end);
            if (ImGui.MenuItem(nameof(BouncySoftCollision))) result = PropertiesWindow.DefaultCollision<BouncySoftCollision>(start, end);
            if (ImGui.MenuItem(nameof(BouncyNoSlideCollision))) result = PropertiesWindow.DefaultCollision<BouncyNoSlideCollision>(start, end);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Special Collision"))
        {
            if (ImGui.MenuItem(nameof(StickyCollision))) result = PropertiesWindow.DefaultCollision<StickyCollision>(start, end);
            if (ImGui.MenuItem(nameof(ItemIgnoreCollision))) result = PropertiesWindow.DefaultCollision<ItemIgnoreCollision>(start, end);
            if (ImGui.MenuItem(nameof(TriggerCollision))) result = PropertiesWindow.DefaultCollision<TriggerCollision>(start, end);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Gamemode collision"))
        {
            if (ImGui.MenuItem(nameof(GameModeHardCollision))) result = PropertiesWindow.DefaultCollision<GameModeHardCollision>(start, end);
            if (ImGui.MenuItem(nameof(PressurePlateCollision))) result = PropertiesWindow.DefaultCollision<PressurePlateCollision>(start, end);
            if (ImGui.MenuItem(nameof(SoftPressurePlateCollision))) result = PropertiesWindow.DefaultCollision<SoftPressurePlateCollision>(start, end);
            if (ImGui.MenuItem(nameof(LavaCollision))) result = PropertiesWindow.DefaultCollision<LavaCollision>(start, end);
            ImGui.EndMenu();
        }
        return result;
    }

    public static Maybe<AbstractItemSpawn> AddItemSpawnMenu(Vector2 pos)
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.MenuItem(nameof(ItemSpawn))) result = PropertiesWindow.DefaultItemSpawn<ItemSpawn>(pos);
        if (ImGui.MenuItem(nameof(ItemInitSpawn))) result = PropertiesWindow.DefaultItemSpawn<ItemInitSpawn>(pos);
        if (ImGui.MenuItem(nameof(TeamItemInitSpawn))) result = PropertiesWindow.DefaultItemSpawn<TeamItemInitSpawn>(pos);
        if (ImGui.MenuItem(nameof(ItemSet))) result = PropertiesWindow.DefaultItemSpawn<ItemSet>(pos);
        return result;
    }

    public static Maybe<AbstractAsset> AddAssetMenu(Vector2 pos, bool allowAsset = false)
    {
        Maybe<AbstractAsset> result = new();
        if (allowAsset && ImGui.MenuItem("Asset")) result = PropertiesWindow.DefaultAsset(pos);
        if (ImGui.MenuItem("Platform with AssetName")) result = PropertiesWindow.DefaultPlatformWithAssetName(pos);
        if (ImGui.MenuItem("Platform without AssetName")) result = PropertiesWindow.DefaultPlatformWithoutAssetName(pos);
        return result;
    }

    private static void AddObjectWithDynamicMenuHistory<N, D>(Vector2 pos, string dynName, Func<Vector2, Maybe<N>> normalMenu, Action<N> normalCmdAdd, Action<D> dynamicCmdAdd, SelectionContext selection, CommandHistory cmd)
        where D : AbstractDynamic<N>, new()
        where N : ISerializable, IDeserializable, IDrawable
    {
        Maybe<N> maybeItem = normalMenu(pos);
        if (maybeItem.TryGetValue(out N? item))
        {
            normalCmdAdd(item);
            cmd.SetAllowMerge(false);
            selection.Object = item;
        }
        if (ImGui.MenuItem(dynName))
        {
            D dynamic = new() { X = pos.X, Y = pos.Y, Children = [], PlatID = "0" };
            dynamicCmdAdd(dynamic);
            cmd.SetAllowMerge(false);
            selection.Object = dynamic;
        }
    }

    public static void AddDynamicItemSpawnMenuHistory(Vector2 pos, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<AbstractItemSpawn, DynamicItemSpawn>(pos, "DynamicItemSpawn", AddItemSpawnMenu,
            newVal => cmd.Add(new ArrayAddCommand<AbstractItemSpawn>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicItemSpawn>(val => l.Desc.DynamicItemSpawns = val, l.Desc.DynamicItemSpawns, newVal)),
            selection, cmd);

    public static void AddDynamicRespawnMenuHistory(Vector2 pos, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<Respawn, DynamicRespawn>(pos, "DynamicRespawn",
            position => ImGui.MenuItem("Respawn") ? PropertiesWindow.DefaultRespawn(position) : Maybe<Respawn>.None,
            newVal => cmd.Add(new ArrayAddCommand<Respawn>(val => l.Desc.Respawns = val, l.Desc.Respawns, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicRespawn>(val => l.Desc.DynamicRespawns = val, l.Desc.DynamicRespawns, newVal)),
            selection, cmd);

    private static Maybe<AbstractCollision> AddCollisionMenu_(Vector2 pos) => AddCollisionMenu(pos, pos with { X = pos.X + 100 });

    public static void AddDynamicCollisionMenuHistory(Vector2 pos, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<AbstractCollision, DynamicCollision>(pos, "DynamicCollision", AddCollisionMenu_,
            newVal => cmd.Add(new ArrayAddCommand<AbstractCollision>(val => l.Desc.Collisions = val, l.Desc.Collisions, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicCollision>(val => l.Desc.DynamicCollisions = val, l.Desc.DynamicCollisions, newVal)),
            selection, cmd);

    public static void AddDynamicNavNodeMenuHistory(Vector2 pos, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<NavNode, DynamicNavNode>(pos, "DynamicNavNode",
            position => ImGui.MenuItem("NavNode") ? PropertiesWindow.DefaultNavNode(position, l.Desc) : Maybe<NavNode>.None,
            newVal => cmd.Add(new ArrayAddCommand<NavNode>(val => l.Desc.NavNodes = val, l.Desc.NavNodes, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicNavNode>(val => l.Desc.DynamicNavNodes = val, l.Desc.DynamicNavNodes, newVal)),
            selection, cmd);

    public static void AddMovingPlatformMenuHistory(Vector2 pos, Level l, SelectionContext selection, CommandHistory cmd)
    {
        Maybe<AbstractAsset> maybeAsset = AddAssetMenu(pos, allowAsset: false);
        if (ImGui.MenuItem("MovingPlatform"))
        {
            maybeAsset = new MovingPlatform()
            {
                PlatID = "0",
                X = pos.X,
                Y = pos.Y,
                Assets = [],
                ScaleX = 1,
                ScaleY = 1,
                Animation = new() { NumFrames = 1, KeyFrames = [new KeyFrame() { X = 0, Y = 0, FrameNum = 1 }] }
            };
        }

        if (maybeAsset.TryGetValue(out AbstractAsset? asset))
        {
            cmd.Add(new ArrayAddCommand<AbstractAsset>(val => l.Desc.Assets = val, l.Desc.Assets, asset));
            cmd.SetAllowMerge(false);
            selection.Object = asset;
        }
    }
}