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
        ImGui.Text($"FileName: {l.Type.FileName}");

        l.Type.DisplayName = ImGuiExt.InputText("DisplayName", l.Type.DisplayName);

        if (ImGui.CollapsingHeader("Kill Bounds##overview") && l.Type.LevelName != "Random")
        {
            _propChanged |= ImGuiExt.DragIntHistory("Top##killbounds", (int)l.Type.TopKill!, (val) => l.Type.TopKill = val, cmd, minValue: 1);
            _propChanged |= ImGuiExt.DragIntHistory("Bottom##killbounds", (int)l.Type.BottomKill!, (val) => l.Type.BottomKill = val, cmd, minValue: 1);
            _propChanged |= ImGuiExt.DragIntHistory("Left##killbounds", (int)l.Type.LeftKill!, (val) => l.Type.LeftKill = val, cmd, minValue: 1);
            _propChanged |= ImGuiExt.DragIntHistory("Right##killbounds", (int)l.Type.RightKill!, (val) => l.Type.RightKill = val, cmd, minValue: 1);
        }

        if (ImGui.CollapsingHeader("Camera Bounds##overview"))
        {
            _propChanged |= ImGuiExt.DragFloatHistory("x##cambounds", l.Desc.CameraBounds.X, (val) => l.Desc.CameraBounds.X = val, cmd);
            _propChanged |= ImGuiExt.DragFloatHistory("y##cambounds", l.Desc.CameraBounds.Y, (val) => l.Desc.CameraBounds.Y = val, cmd);
            _propChanged |= ImGuiExt.DragFloatHistory("w##cambounds", l.Desc.CameraBounds.W, (val) => l.Desc.CameraBounds.W = val, cmd, minValue: 1);
            _propChanged |= ImGuiExt.DragFloatHistory("h##cambounds", l.Desc.CameraBounds.H, (val) => l.Desc.CameraBounds.H = val, cmd, minValue: 1);
        }

        if (ImGui.CollapsingHeader("Spawn Bot Bounds"))
        {
            _propChanged |= ImGuiExt.DragFloatHistory("x##botbounds", l.Desc.SpawnBotBounds.X, (val) => l.Desc.SpawnBotBounds.X = val, cmd);
            _propChanged |= ImGuiExt.DragFloatHistory("y##botbounds", l.Desc.SpawnBotBounds.Y, (val) => l.Desc.SpawnBotBounds.Y = val, cmd);
            _propChanged |= ImGuiExt.DragFloatHistory("w##botbounds", l.Desc.SpawnBotBounds.W, (val) => l.Desc.SpawnBotBounds.W = val, cmd, minValue: 1);
            _propChanged |= ImGuiExt.DragFloatHistory("h##botbounds", l.Desc.SpawnBotBounds.H, (val) => l.Desc.SpawnBotBounds.H = val, cmd, minValue: 1);
        }

        if (ImGui.CollapsingHeader("Weapon Spawn Color##overview") && l.Type.CrateColorA is not null && l.Type.CrateColorB is not null)
        {
            Color colA = ImGuiExt.ColorPicker3("Outer##crateColorA", new(l.Type.CrateColorA.Value.R, l.Type.CrateColorA.Value.G, l.Type.CrateColorA.Value.B, 255));
            Color colB = ImGuiExt.ColorPicker3("Inner##crateColorB", new(l.Type.CrateColorB.Value.R, l.Type.CrateColorB.Value.G, l.Type.CrateColorB.Value.B, 255));
            CrateColor crateColA = new(colA.R, colA.G, colA.B);
            CrateColor crateColB = new(colB.R, colB.G, colB.B);

            if (crateColA != l.Type.CrateColorA)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColA, false));
            }

            if (crateColB != l.Type.CrateColorB)
            {
                _propChanged = true;
                cmd.Add(new CrateColorChange(l.Type, crateColB, true));
            }
        }

        // TODO: background and thumbnail
        if (ImGui.CollapsingHeader("Images"))
        {

        }

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Platforms##overview"))
            ShowSelectableList(l.Desc.Assets, ref selected);

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
            if (ImGui.Selectable($"{o.GetType().Name} {GetExtraObjectInfo(o)}##selectable{o.GetHashCode()}", selected == o))
                selected = o;
        }
    }

    private static string GetExtraObjectInfo(object o) => o switch
    {
        Platform p => $"({p.InstanceName})",
        MovingPlatform mp => $"({mp.PlatID})",
        Respawn r => $"({r.X}, {r.Y})",
        AbstractItemSpawn i => $"({i.X}, {i.Y}, {i.W}, {i.H})",
        AbstractCollision c => $"({c.X1}, {c.Y1}, {c.X2}, {c.Y2})",
        _ => ""
    };
}