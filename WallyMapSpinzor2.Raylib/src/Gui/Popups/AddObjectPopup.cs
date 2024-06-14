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
                if (ImGui.BeginMenu("Normal Collision"))
                {
                    AddCollisionMenuItem<HardCollision>(nameof(HardCollision), l, cmd);
                    AddCollisionMenuItem<SoftCollision>(nameof(SoftCollision), l, cmd);
                    AddCollisionMenuItem<NoSlideCollision>(nameof(NoSlideCollision), l, cmd);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Bouncy Collision"))
                {
                    AddCollisionMenuItem<BouncyHardCollision>(nameof(BouncyHardCollision), l, cmd);
                    AddCollisionMenuItem<BouncySoftCollision>(nameof(BouncySoftCollision), l, cmd);
                    AddCollisionMenuItem<BouncyNoSlideCollision>(nameof(BouncyNoSlideCollision), l, cmd);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Special Collision"))
                {
                    AddCollisionMenuItem<StickyCollision>(nameof(StickyCollision), l, cmd);
                    AddCollisionMenuItem<ItemIgnoreCollision>(nameof(ItemIgnoreCollision), l, cmd);
                    AddCollisionMenuItem<TriggerCollision>(nameof(TriggerCollision), l, cmd);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Gamemode collision"))
                {
                    AddCollisionMenuItem<GameModeHardCollision>(nameof(GameModeHardCollision), l, cmd);
                    AddCollisionMenuItem<PressurePlateCollision>(nameof(PressurePlateCollision), l, cmd);
                    AddCollisionMenuItem<SoftPressurePlateCollision>(nameof(SoftPressurePlateCollision), l, cmd);
                    AddCollisionMenuItem<LavaCollision>(nameof(LavaCollision), l, cmd);
                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("ItemSpawn"))
            {
                AddItemSpawnMenuItem<ItemSpawn>(nameof(ItemSpawn), l, cmd);
                AddItemSpawnMenuItem<ItemInitSpawn>(nameof(ItemInitSpawn), l, cmd);
                AddItemSpawnMenuItem<TeamItemInitSpawn>(nameof(TeamItemInitSpawn), l, cmd);
                AddItemSpawnMenuItem<ItemSet>(nameof(ItemSet), l, cmd);
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
                Platform? p = null;
                if (ImGui.MenuItem("With AssetName"))
                {
                    p = PropertiesWindow.DefaultPlatformWithAssetName;
                }
                if (ImGui.MenuItem("Without AssetName"))
                {
                    p = PropertiesWindow.DefaultPlatformWithoutAssetName;
                }

                if (p is not null)
                {
                    cmd.Add(new PropChangeCommand<AbstractAsset[]>(val => l.Desc.Assets = val, l.Desc.Assets, [.. l.Desc.Assets, p]));
                    cmd.SetAllowMerge(false);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private static void AddCollisionMenuItem<T>(string title, Level l, CommandHistory cmd)
        where T : AbstractCollision, new()
    {
        if (ImGui.MenuItem(title))
        {
            AddCollision<T>(l, cmd);
            ImGui.CloseCurrentPopup();
        }
    }

    private static void AddCollision<T>(Level l, CommandHistory cmd)
        where T : AbstractCollision, new()
    {
        T col = PropertiesWindow.DefaultCollision<T>(NewPos);
        cmd.Add(new PropChangeCommand<AbstractCollision[]>(val => l.Desc.Collisions = val, l.Desc.Collisions, [.. l.Desc.Collisions, col]));
        cmd.SetAllowMerge(false);
    }

    private static void AddItemSpawnMenuItem<T>(string title, Level l, CommandHistory cmd)
        where T : AbstractItemSpawn, new()
    {
        if (ImGui.MenuItem(title))
        {
            AddItemSpawn<T>(l, cmd);
            ImGui.CloseCurrentPopup();
        }
    }

    private static void AddItemSpawn<T>(Level l, CommandHistory cmd)
        where T : AbstractItemSpawn, new()
    {
        T spawn = PropertiesWindow.DefaultItemSpawn<T>(NewPos);
        cmd.Add(new PropChangeCommand<AbstractItemSpawn[]>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, [.. l.Desc.ItemSpawns, spawn]));
        cmd.SetAllowMerge(false);
    }
}