using System.Collections;
using ImGuiNET;
using Rl = Raylib_cs.Raylib;
using Raylib_cs;

namespace WallyMapSpinzor2.Raylib;

public class MapOverviewWindow
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(Level l, CommandHistory cmd, ref object? selected)
    {
        ImGui.Begin("Map Overview", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        if (l.Type is null) return;

        ImGui.Text($"LevelName: {l.Type.LevelName}");
        ImGui.Text($"AssetName: {l.Type.AssetName}");
        ImGui.Text($"DisplayName: {l.Type.DisplayName}");
        ImGui.Text($"FileName: {l.Type.FileName}");

        if (ImGui.CollapsingHeader("Kill Bounds##overview"))
        {
            if (l.Type.LevelName != "Random")
            {
                int top = ImGuiExt.DragInt("Top##killbounds", (int)l.Type.TopKill!, minValue: 1) - (int)l.Type.TopKill;
                int bot = ImGuiExt.DragInt("Bottom##killbounds", (int)l.Type.BottomKill!, minValue: 1) - (int)l.Type.BottomKill;
                int left = ImGuiExt.DragInt("Left##killbounds", (int)l.Type.LeftKill!, minValue: 1) - (int)l.Type.LeftKill;
                int right = ImGuiExt.DragInt("Right##killbounds", (int)l.Type.RightKill!, minValue: 1) - (int)l.Type.RightKill;

                if (top != 0 || bot != 0 || left != 0 || right != 0)
                {
                    _propChanged = true;
                    cmd.Add(new KillBoundsChange(l.Type, top, bot, left, right));
                }
            }
        }

        if (ImGui.CollapsingHeader("Camera Bounds##overview"))
        {
            double x = ImGuiExt.DragFloat("x##cambounds", (float)l.Desc.CameraBounds.X) - (float)l.Desc.CameraBounds.X;
            double y = ImGuiExt.DragFloat("y##cambounds", (float)l.Desc.CameraBounds.Y) - (float)l.Desc.CameraBounds.Y;
            double w = ImGuiExt.DragFloat("w##cambounds", (float)l.Desc.CameraBounds.W, minValue: 1) - (float)l.Desc.CameraBounds.W;
            double h = ImGuiExt.DragFloat("h##cambounds", (float)l.Desc.CameraBounds.H, minValue: 1) - (float)l.Desc.CameraBounds.H;

            if (x != 0 || y != 0 || w != 0 || h != 0)
            {
                _propChanged = true;
                cmd.Add(new CameraboundsChange(l.Desc.CameraBounds, x, y, w, h));
            }
        }

        if (ImGui.CollapsingHeader("Spawn Bot Bounds"))
        {
            double x = ImGuiExt.DragFloat("x##spawnbotbounds", (float)l.Desc.SpawnBotBounds.X) - (float)l.Desc.SpawnBotBounds.X;
            double y = ImGuiExt.DragFloat("y##spawnbotbounds", (float)l.Desc.SpawnBotBounds.Y) - (float)l.Desc.SpawnBotBounds.Y;
            double w = ImGuiExt.DragFloat("w##spawnbotbounds", (float)l.Desc.SpawnBotBounds.W, minValue: 1) - (float)l.Desc.SpawnBotBounds.W;
            double h = ImGuiExt.DragFloat("h##spawnbotbounds", (float)l.Desc.SpawnBotBounds.H, minValue: 1) - (float)l.Desc.SpawnBotBounds.H;

            if (x != 0 || y != 0 || w != 0 || h != 0)
            {
                _propChanged = true;
                cmd.Add(new BotBoundsChange(l.Desc.SpawnBotBounds, x, y, w, h));
            }
        }

        if (ImGui.CollapsingHeader("Weapon Spawn Color##overview"))
        {
            l.Type.CrateColorA ??= new(0, 0, 0);
            l.Type.CrateColorB ??= new(0, 0, 0);
            Color colA = ImGuiExt.ColorPicker3("Outer##crateColorA", new(l.Type.CrateColorA.Value.R, l.Type.CrateColorA.Value.G, l.Type.CrateColorA.Value.B, 255));
            Color colB = ImGuiExt.ColorPicker3("Inner##crateColorB", new(l.Type.CrateColorB.Value.R, l.Type.CrateColorB.Value.G, l.Type.CrateColorB.Value.B, 255));
            l.Type.CrateColorA = new(colA.R, colA.G, colA.B);
            l.Type.CrateColorB = new(colB.R, colB.G, colB.B);
        }

        // TODO: background and thumbnail
        if (ImGui.CollapsingHeader("Images"))
        {

        }

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Collisions##overview"))
        {
            ShowSelectableList(l.Desc.Collisions, ref selected);
            ShowSelectableList(l.Desc.DynamicCollisions, ref selected);
        }

        if (ImGui.CollapsingHeader("Respawns##overview"))
        {
            ShowSelectableList(l.Desc.Respawns, ref selected);
            ShowSelectableList(l.Desc.DynamicRespawns, ref selected);
        }

        if (ImGui.CollapsingHeader("Item Spawns##overview"))
        {
            ShowSelectableList(l.Desc.ItemSpawns, ref selected);
            ShowSelectableList(l.Desc.DynamicItemSpawns, ref selected);
        }

        ImGui.End();
    }

    private static void ShowSelectableList(IEnumerable list, ref object? selected)
    {
        foreach (object o in list)
        {
            if (ImGui.Selectable($"{o.GetType().Name}##selectable{o.GetHashCode()}", selected == o))
                selected = o;
        }
    }
}