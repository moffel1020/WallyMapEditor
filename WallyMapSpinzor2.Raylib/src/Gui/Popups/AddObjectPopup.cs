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
            ImGui.Text("Add new object");
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
                Respawn res = new() { X = NewPos.X, Y = NewPos.Y };
                cmd.Add(new PropChangeCommand<Respawn[]>(val => l.Desc.Respawns = val, l.Desc.Respawns, [.. l.Desc.Respawns, res]));
                cmd.SetAllowMerge(false);
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.MenuItem("Platform"))
            {
                Platform? p = null;
                if (ImGui.MenuItem("With AssetName"))
                {
                    p = new()
                    {
                        InstanceName = "Custom_Platform",
                        AssetName = "../Battlehill/SK_Small_Plat.png",
                        X = NewPos.X,
                        Y = NewPos.Y,
                        W = 750,
                        H = 175,
                    };
                }
                if (ImGui.MenuItem("Without AssetName"))
                {
                    p = new()
                    {
                        InstanceName = "Custom_Platform",
                        AssetChildren = [],
                        X = NewPos.X,
                        Y = NewPos.Y,
                        ScaleX = 1,
                        ScaleY = 1,
                    };
                }

                if (p is not null)
                {
                    cmd.Add(new PropChangeCommand<AbstractAsset[]>(val => l.Desc.Assets = val, l.Desc.Assets, [.. l.Desc.Assets, p]));
                    cmd.SetAllowMerge(false);
                    ImGui.CloseCurrentPopup();
                }
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
        T col = new() { X1 = NewPos.X, X2 = NewPos.X + 100, Y1 = NewPos.Y, Y2 = NewPos.Y };
        if (col is AbstractPressurePlateCollision pcol)
        {
            pcol.AssetName = "a__AnimationPressurePlate";
            pcol.FireOffsetX = [];
            pcol.FireOffsetY = [];
            pcol.TrapPowers = [];
            pcol.AnimOffsetX = (col.X1 + col.X2) / 2;
            pcol.AnimOffsetY = (col.Y1 + col.Y2) / 2;
            pcol.Cooldown = 3000;
        }
        if (col is LavaCollision lcol)
        {
            lcol.LavaPower = "LavaBurn";
        }
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
        T spawn = new() { X = NewPos.X, Y = NewPos.Y };
        (spawn.W, spawn.H) = (100, 100);
        cmd.Add(new PropChangeCommand<AbstractItemSpawn[]>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, [.. l.Desc.ItemSpawns, spawn]));
        cmd.SetAllowMerge(false);
    }
}