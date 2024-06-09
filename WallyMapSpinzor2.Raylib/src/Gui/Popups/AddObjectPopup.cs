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
                if (ImGui.MenuItem("Soft Collision"))
                {
                    AddCollision<SoftCollision>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("Hard Collision"))
                {
                    AddCollision<HardCollision>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("ItemSpawn"))
            {
                if (ImGui.MenuItem("ItemSpawn"))
                {
                    AddItemSpawn<ItemSpawn>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("ItemInitSpawn"))
                {
                    AddItemSpawn<ItemInitSpawn>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("TeamItemInitSpawn"))
                {
                    AddItemSpawn<TeamItemInitSpawn>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.MenuItem("ItemSet"))
                {
                    AddItemSpawn<ItemSet>(l, cmd);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Respawn"))
            {
                Respawn res = new(){X = NewPos.X, Y = NewPos.Y};
                cmd.Add(new PropChangeCommand<Respawn[]>(val => l.Desc.Respawns = val, l.Desc.Respawns, [.. l.Desc.Respawns, res]));
                cmd.SetAllowMerge(false);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private static void AddCollision<T>(Level l, CommandHistory cmd)
        where T : AbstractCollision, new()
    {
        T col = new(){ X1 = NewPos.X, X2 = NewPos.X + 100, Y1 = NewPos.Y, Y2 = NewPos.Y };
        cmd.Add(new PropChangeCommand<AbstractCollision[]>(val => l.Desc.Collisions = val, l.Desc.Collisions, [.. l.Desc.Collisions, col]));
        cmd.SetAllowMerge(false);
    }

    private static void AddItemSpawn<T>(Level l, CommandHistory cmd)
        where T : AbstractItemSpawn, new()
    {
        T spawn = new(){ X = NewPos.X, Y = NewPos.Y };
        (spawn.W, spawn.H) = (spawn.DefaultW, spawn.DefaultH);
        cmd.Add(new PropChangeCommand<AbstractItemSpawn[]>(val => l.Desc.ItemSpawns = val, l.Desc.ItemSpawns, [.. l.Desc.ItemSpawns, spawn]));
        cmd.SetAllowMerge(false);
    }
}