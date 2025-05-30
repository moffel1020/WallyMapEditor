using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowRespawnProps(Respawn r, EditorLevel level)
    {
        SelectionContext selection = level.Selection;
        CommandHistory cmd = level.CommandHistory;

        if (r.Parent is not null)
        {
            ImGui.Text($"Parent DynamicRespawn: ");
            ImGui.SameLine();
            if (ImGui.Button($"PlatID {r.Parent.PlatID}"))
                selection.Object = r.Parent;
            ImGui.Separator();
        }

        bool propChanged = false;

        RemoveButton(r, level);
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

    public static Respawn DefaultRespawn(double posX, double posY) => new() { X = posX, Y = posY };
}