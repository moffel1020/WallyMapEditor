namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public bool ShowRespawnProps(Respawn r, CommandHistory cmd)
    {
        _propChanged |= ImGuiExt.DragFloatHistory($"x", r.X, (val) => r.X = val, cmd);
        _propChanged |= ImGuiExt.DragFloatHistory($"y", r.Y, (val) => r.Y = val, cmd);

        if (r.ExpandedInit && r.Initial) r.Initial = false;
        ImGuiExt.WithDisabled(r.ExpandedInit, () => r.Initial = ImGuiExt.Checkbox($"Initial##props{r.GetHashCode()}", r.Initial));
        ImGuiExt.WithDisabled(r.Initial, () => r.ExpandedInit = ImGuiExt.Checkbox($"ExpandedInit##props{r.GetHashCode()}", r.ExpandedInit));

        return true;
    }
}