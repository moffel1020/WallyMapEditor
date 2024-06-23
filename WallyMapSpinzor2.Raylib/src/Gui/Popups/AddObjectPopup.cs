using System.Numerics;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public static class AddObjectPopup
{
    public const string NAME = "addobject";

    public static Vector2 NewPos { get; set; }

    public static void Update(Level l, CommandHistory cmd)
    {
        if (ImGui.BeginPopup(NAME))
        {
            ImGui.SeparatorText("Add new object");
            if (ImGui.BeginMenu("Collision"))
            {
                Maybe<AbstractCollision> maybeAc = AddCollisionMenu(NewPos);
                if (maybeAc.TryGetValue(out AbstractCollision? ac))
                {
                    cmd.Add(new PropChangeCommand<AbstractCollision[]>(val => l.Desc.Collisions = val, l.Desc.Collisions, [.. l.Desc.Collisions, ac]));
                    cmd.SetAllowMerge(false);
                }

                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("ItemSpawn"))
            {
                Maybe<AbstractItemSpawn> maybeItem = AddItemSpawnMenu(NewPos);
                if (maybeItem.TryGetValue(out AbstractItemSpawn? item))
                {
                    cmd.Add(new PropChangeCommand<AbstractItemSpawn[]>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, [.. l.Desc.ItemSpawns, item]));
                    cmd.SetAllowMerge(false);
                }

                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Respawn"))
            {
                Respawn res = PropertiesWindow.DefaultRespawn(NewPos);
                cmd.Add(new PropChangeCommand<Respawn[]>(val => l.Desc.Respawns = val, l.Desc.Respawns, [.. l.Desc.Respawns, res]));
                cmd.SetAllowMerge(false);
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.BeginMenu("Platform"))
            {
                Maybe<AbstractAsset> maybeAsset = AddAssetMenu(NewPos);
                if (maybeAsset.TryGetValue(out AbstractAsset? asset))
                {
                    cmd.Add(new PropChangeCommand<AbstractAsset[]>(val => l.Desc.Assets = val, l.Desc.Assets, [.. l.Desc.Assets, asset]));
                    cmd.SetAllowMerge(false);
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    public static Maybe<AbstractCollision> AddCollisionMenu(Vector2 pos)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.BeginMenu("Normal Collision"))
        {
            if (ImGui.MenuItem(nameof(HardCollision))) result = PropertiesWindow.DefaultCollision<HardCollision>(pos);
            if (ImGui.MenuItem(nameof(SoftCollision))) result = PropertiesWindow.DefaultCollision<SoftCollision>(pos);
            if (ImGui.MenuItem(nameof(NoSlideCollision))) result = PropertiesWindow.DefaultCollision<NoSlideCollision>(pos);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Bouncy Collision"))
        {
            if (ImGui.MenuItem(nameof(BouncyHardCollision))) result = PropertiesWindow.DefaultCollision<BouncyHardCollision>(pos);
            if (ImGui.MenuItem(nameof(BouncySoftCollision))) result = PropertiesWindow.DefaultCollision<BouncySoftCollision>(pos);
            if (ImGui.MenuItem(nameof(BouncyNoSlideCollision))) result = PropertiesWindow.DefaultCollision<BouncyNoSlideCollision>(pos);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Special Collision"))
        {
            if (ImGui.MenuItem(nameof(StickyCollision))) result = PropertiesWindow.DefaultCollision<StickyCollision>(pos);
            if (ImGui.MenuItem(nameof(ItemIgnoreCollision))) result = PropertiesWindow.DefaultCollision<ItemIgnoreCollision>(pos);
            if (ImGui.MenuItem(nameof(TriggerCollision))) result = PropertiesWindow.DefaultCollision<TriggerCollision>(pos);
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Gamemode collision"))
        {
            if (ImGui.MenuItem(nameof(GameModeHardCollision))) result = PropertiesWindow.DefaultCollision<GameModeHardCollision>(pos);
            if (ImGui.MenuItem(nameof(PressurePlateCollision))) result = PropertiesWindow.DefaultCollision<PressurePlateCollision>(pos);
            if (ImGui.MenuItem(nameof(SoftPressurePlateCollision))) result = PropertiesWindow.DefaultCollision<SoftPressurePlateCollision>(pos);
            if (ImGui.MenuItem(nameof(LavaCollision))) result = PropertiesWindow.DefaultCollision<LavaCollision>(pos);
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
}