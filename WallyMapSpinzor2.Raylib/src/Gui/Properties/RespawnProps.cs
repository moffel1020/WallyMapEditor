namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public bool ShowRespawnProps(Respawn r, CommandHistory cmd)
    {
        double x = ImGuiExt.DragFloat($"x##props{r.GetHashCode()}", (float)r.X) - (float)r.X;
        double y = ImGuiExt.DragFloat($"y##props{r.GetHashCode()}", (float)r.Y) - (float)r.Y;

        if (x != 0 || y != 0)
        {
            _propChanged = true;
            cmd.Add(new RespawnMove(r, x, y));
        }

        if (r.ExpandedInit && r.Initial) r.Initial = false;
        ImGuiExt.WithDisabled(r.ExpandedInit, () => r.Initial = ImGuiExt.Checkbox($"Initial##props{r.GetHashCode()}", r.Initial));
        ImGuiExt.WithDisabled(r.Initial, () => r.ExpandedInit = ImGuiExt.Checkbox($"ExpandedInit##props{r.GetHashCode()}", r.ExpandedInit));

        return true;
    }
}