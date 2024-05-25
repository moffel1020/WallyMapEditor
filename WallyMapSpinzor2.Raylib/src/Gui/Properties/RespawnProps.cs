namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowRespawnProps(Respawn r, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragFloatHistory("X", r.X, val => r.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", r.Y, val => r.Y = val, cmd);

        if (r.ExpandedInit && r.Initial) r.Initial = false;
        ImGuiExt.WithDisabled(r.ExpandedInit, () =>
        {
            propChanged |= ImGuiExt.CheckboxHistory("Initial", r.Initial, val => r.Initial = val, cmd);
        });
        ImGuiExt.WithDisabled(r.Initial, () =>
        {
            propChanged |= ImGuiExt.CheckboxHistory("ExpandedInit", r.ExpandedInit, val => r.ExpandedInit = val, cmd);
        });

        return propChanged;
    }
}