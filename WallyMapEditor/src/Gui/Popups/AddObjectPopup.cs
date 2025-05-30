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

    public static void Update(EditorLevel level)
    {
        Level l = level.Level;
        CommandHistory cmd = level.CommandHistory;
        SelectionContext selection = level.Selection;

        if (_shouldOpen)
        {
            ImGui.OpenPopup(NAME);
            _shouldOpen = false;
        }

        if (!ImGui.BeginPopup(NAME)) return;

        ImGui.SeparatorText("Add new object");

        (float posX, float posY) = (NewPos.X, NewPos.Y);
        if (ImGui.BeginMenu("Collisions")) { AddDynamicCollisionMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("ItemSpawns")) { AddDynamicItemSpawnMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Respawns")) { AddDynamicRespawnMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Platforms")) { AddMovingPlatformMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("NavNodes")) { AddDynamicNavNodeMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }
        if (ImGui.BeginMenu("Volumes")) { AddVolumeMenuHistory(posX, posY, l, selection, cmd); ImGui.EndMenu(); }

        ImGui.EndPopup();
    }

    public static Maybe<AbstractCollision> AddCollisionMenu(double startX, double startY, double endX, double endY)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.BeginMenu("Normal Collision"))
        {
            if (ImGui.MenuItem(nameof(HardCollision))) result = PropertiesWindow.DefaultCollision<HardCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(SoftCollision))) result = PropertiesWindow.DefaultCollision<SoftCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(NoSlideCollision))) result = PropertiesWindow.DefaultCollision<NoSlideCollision>(startX, startY, endX, endY);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Bouncy Collision"))
        {
            if (ImGui.MenuItem(nameof(BouncyHardCollision))) result = PropertiesWindow.DefaultCollision<BouncyHardCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(BouncySoftCollision))) result = PropertiesWindow.DefaultCollision<BouncySoftCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(BouncyNoSlideCollision))) result = PropertiesWindow.DefaultCollision<BouncyNoSlideCollision>(startX, startY, endX, endY);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Special Collision"))
        {
            if (ImGui.MenuItem(nameof(StickyCollision))) result = PropertiesWindow.DefaultCollision<StickyCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(ItemIgnoreCollision))) result = PropertiesWindow.DefaultCollision<ItemIgnoreCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(TriggerCollision))) result = PropertiesWindow.DefaultCollision<TriggerCollision>(startX, startY, endX, endY);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Gamemode collision"))
        {
            if (ImGui.MenuItem(nameof(GameModeHardCollision))) result = PropertiesWindow.DefaultCollision<GameModeHardCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(PressurePlateCollision))) result = PropertiesWindow.DefaultCollision<PressurePlateCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(SoftPressurePlateCollision))) result = PropertiesWindow.DefaultCollision<SoftPressurePlateCollision>(startX, startY, endX, endY);
            if (ImGui.MenuItem(nameof(LavaCollision))) result = PropertiesWindow.DefaultCollision<LavaCollision>(startX, startY, endX, endY);
            ImGui.EndMenu();
        }
        return result;
    }

    public static Maybe<AbstractItemSpawn> AddItemSpawnMenu(double posX, double posY)
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.MenuItem(nameof(ItemSpawn))) result = PropertiesWindow.DefaultItemSpawn<ItemSpawn>(posX, posY);
        if (ImGui.MenuItem(nameof(ItemInitSpawn))) result = PropertiesWindow.DefaultItemSpawn<ItemInitSpawn>(posX, posY);
        if (ImGui.MenuItem(nameof(TeamItemInitSpawn))) result = PropertiesWindow.DefaultItemSpawn<TeamItemInitSpawn>(posX, posY);
        if (ImGui.MenuItem(nameof(ItemSet))) result = PropertiesWindow.DefaultItemSpawn<ItemSet>(posX, posY);
        return result;
    }

    public static Maybe<AbstractAsset> AddAssetMenu(double posX, double posY, bool allowAsset = false)
    {
        Maybe<AbstractAsset> result = new();
        if (allowAsset && ImGui.MenuItem("Asset")) result = PropertiesWindow.DefaultAsset(posX, posY);
        if (ImGui.MenuItem("Platform with AssetName")) result = PropertiesWindow.DefaultPlatformWithAssetName(posX, posY);
        if (ImGui.MenuItem("Platform without AssetName")) result = PropertiesWindow.DefaultPlatformWithoutAssetName(posX, posY);
        return result;
    }

    public static Maybe<AbstractVolume> AddVolumeMenu(double x, double y)
    {
        Maybe<AbstractVolume> result = new();
        if (ImGui.MenuItem("Goal")) result = PropertiesWindow.DefaultVolume<Goal>((int)x, (int)y, 100, 100);
        if (ImGui.MenuItem("Volume (plain)")) result = PropertiesWindow.DefaultVolume<Volume>((int)x, (int)y, 100, 100);
        if (ImGui.MenuItem("NoDodgeZone")) result = PropertiesWindow.DefaultVolume<NoDodgeZone>((int)x, (int)y, 100, 100);
        return result;
    }

    private static void AddObjectWithDynamicMenuHistory<N, D>(double posX, double posY, string dynName, Func<double, double, Maybe<N>> normalMenu, Action<N> normalCmdAdd, Action<D> dynamicCmdAdd, SelectionContext selection, CommandHistory cmd)
        where D : AbstractDynamic<N>, new()
        where N : ISerializable, IDeserializable, IDrawable
    {
        Maybe<N> maybeItem = normalMenu(posX, posY);
        if (maybeItem.TryGetValue(out N? item))
        {
            normalCmdAdd(item);
            cmd.SetAllowMerge(false);
            selection.Object = item;
        }
        if (ImGui.MenuItem(dynName))
        {
            D dynamic = new() { X = posX, Y = posY, Children = [], PlatID = "0" };
            dynamicCmdAdd(dynamic);
            cmd.SetAllowMerge(false);
            selection.Object = dynamic;
        }
    }

    private static void AddObjectMenuHistory<N>(double posX, double posY, Func<double, double, Maybe<N>> menu, Action<N> cmdAdd, SelectionContext selection, CommandHistory cmd)
        where N : ISerializable, IDeserializable, IDrawable
    {
        Maybe<N> maybeItem = menu(posX, posY);
        if (maybeItem.TryGetValue(out N? item))
        {
            cmdAdd(item);
            cmd.SetAllowMerge(false);
            selection.Object = item;
        }
    }

    public static void AddDynamicItemSpawnMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<AbstractItemSpawn, DynamicItemSpawn>(posX, posY, "DynamicItemSpawn", AddItemSpawnMenu,
            newVal => cmd.Add(new ArrayAddCommand<AbstractItemSpawn>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicItemSpawn>(val => l.Desc.DynamicItemSpawns = val, l.Desc.DynamicItemSpawns, newVal)),
            selection, cmd);

    public static void AddDynamicRespawnMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<Respawn, DynamicRespawn>(posX, posY, "DynamicRespawn",
            (x, y) => ImGui.MenuItem("Respawn") ? PropertiesWindow.DefaultRespawn(x, y) : Maybe<Respawn>.None,
            newVal => cmd.Add(new ArrayAddCommand<Respawn>(val => l.Desc.Respawns = val, l.Desc.Respawns, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicRespawn>(val => l.Desc.DynamicRespawns = val, l.Desc.DynamicRespawns, newVal)),
            selection, cmd);

    private static Maybe<AbstractCollision> AddCollisionMenu_(double posX, double posY) => AddCollisionMenu(posX, posY, posX + 100, posY);

    public static void AddDynamicCollisionMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<AbstractCollision, DynamicCollision>(posX, posY, "DynamicCollision", AddCollisionMenu_,
            newVal => cmd.Add(new ArrayAddCommand<AbstractCollision>(val => l.Desc.Collisions = val, l.Desc.Collisions, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicCollision>(val => l.Desc.DynamicCollisions = val, l.Desc.DynamicCollisions, newVal)),
            selection, cmd);

    public static void AddDynamicNavNodeMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectWithDynamicMenuHistory<NavNode, DynamicNavNode>(posX, posY, "DynamicNavNode",
            (x, y) => ImGui.MenuItem("NavNode") ? PropertiesWindow.DefaultNavNode(x, y, l.Desc) : Maybe<NavNode>.None,
            newVal => cmd.Add(new ArrayAddCommand<NavNode>(val => l.Desc.NavNodes = val, l.Desc.NavNodes, newVal)),
            newVal => cmd.Add(new ArrayAddCommand<DynamicNavNode>(val => l.Desc.DynamicNavNodes = val, l.Desc.DynamicNavNodes, newVal)),
            selection, cmd);

    public static void AddVolumeMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd) =>
        AddObjectMenuHistory(posX, posY, AddVolumeMenu,
            newVal => cmd.Add(new ArrayAddCommand<AbstractVolume>(val => l.Desc.Volumes = val, l.Desc.Volumes, newVal)),
            selection, cmd);

    public static void AddWaveDataMenuHistory(Level l, SelectionContext selection, CommandHistory cmd)
    {
        WaveData wave = PropertiesWindow.DefaultWaveData(l.Desc);
        cmd.Add(new ArrayAddCommand<WaveData>(val => l.Desc.WaveDatas = val, l.Desc.WaveDatas, wave), false);
        selection.Object = wave;
    }

    public static void AddMovingPlatformMenuHistory(double posX, double posY, Level l, SelectionContext selection, CommandHistory cmd)
    {
        Maybe<AbstractAsset> maybeAsset = AddAssetMenu(posX, posY, allowAsset: false);
        if (ImGui.MenuItem("MovingPlatform"))
        {
            maybeAsset = new MovingPlatform()
            {
                PlatID = "0",
                X = posX,
                Y = posY,
                Assets = [],
                ScaleX = 1,
                ScaleY = 1,
                Animation = new()
                {
                    NumFrames = 1,
                    KeyFrames = [new KeyFrame() { X = 0, Y = 0, FrameNum = 1 }],
                    EasePower = 2,
                },
            };
        }

        if (maybeAsset.TryGetValue(out AbstractAsset? asset))
        {
            cmd.Add(new ArrayAddCommand<AbstractAsset>(val => l.Desc.Assets = val, l.Desc.Assets, asset), false);
            selection.Object = asset;
        }
    }
}