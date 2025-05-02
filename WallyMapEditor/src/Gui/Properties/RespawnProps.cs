using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowRespawnProps(Respawn r, CommandHistory cmd, PropertiesWindowData data)
    {
        if (r.Parent is not null)
        {
            ImGui.Text($"Parent DynamicRespawn: ");
            ImGui.SameLine();
            if (ImGui.Button($"PlatID {r.Parent.PlatID}")) data.Selection.Object = r.Parent;
            ImGui.Separator();
        }

        bool propChanged = false;

        if (data.Level is not null)
            RemoveButton(r, data.Level.Desc, cmd);
        ImGui.Separator();

        propChanged |= ImGuiExt.DragDoubleHistory("X", r.X, val => r.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", r.Y, val => r.Y = val, cmd);

        if (r.ExpandedInit && r.Initial) r.Initial = false;
        using (ImGuiExt.DisabledIf(r.ExpandedInit))
            propChanged |= ImGuiExt.CheckboxHistory("Initial", r.Initial, val => r.Initial = val, cmd);
        using (ImGuiExt.DisabledIf(r.Initial))
            propChanged |= ImGuiExt.CheckboxHistory("ExpandedInit", r.ExpandedInit, val => r.ExpandedInit = val, cmd);

        return propChanged;
    }

    private static Respawn[] GetRespawnParentArray(Respawn r, LevelDesc desc) =>
        r.Parent is null ? desc.Respawns : r.Parent.Children;

    private static Action<Respawn[]> SetRespawnParentArray(Respawn r, LevelDesc desc) =>
        r.Parent is null
            ? val => desc.Respawns = val
            : val => r.Parent.Children = val;

    public static Respawn DefaultRespawn(double posX, double posY) => new() { X = posX, Y = posY };
}