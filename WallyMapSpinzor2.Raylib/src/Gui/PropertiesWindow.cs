using ImGuiNET;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class PropertiesWindow 
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    // TODO: for collision and itemspawns, add the ability to change their types
    public void Show(object o, CommandHistory cmd)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        if (o is AbstractCollision ac)
        {
            // TODO: hardcollision should be edited as a shape rather than an individual collision, if they are not a shape they wont work properly ingame

            // using float casts to avoid imprecision when subtracting
            double x1 = ImGuiExt.DragFloat($"x1##props{ac.GetHashCode()}", (float)ac.X1) - (float)ac.X1;
            double x2 = ImGuiExt.DragFloat($"x2##props{ac.GetHashCode()}", (float)ac.X2) - (float)ac.X2;
            double y1 = ImGuiExt.DragFloat($"y1##props{ac.GetHashCode()}", (float)ac.Y1) - (float)ac.Y1;
            double y2 = ImGuiExt.DragFloat($"y2##props{ac.GetHashCode()}", (float)ac.Y2) - (float)ac.Y2;
            
            if (x1 != 0 || x2 != 0 || y1 != 0 || y2 != 0)
            {
                _propChanged = true;
                cmd.Add(new CollisionMove(ac,x1, x2, y1, y2));
            }
        }

        else if (o is Respawn r)
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
        }

        else if (o is AbstractItemSpawn i)
        {
            double x = ImGuiExt.DragFloat($"x##props{i.GetHashCode}", (float)i.X) - (float)i.X;
            double y = ImGuiExt.DragFloat($"y##props{i.GetHashCode}", (float)i.Y) - (float)i.Y;
            double w = ImGuiExt.DragFloat($"w##props{i.GetHashCode}", (float)i.W, minValue: 1) - (float)i.W;
            double h = ImGuiExt.DragFloat($"h##props{i.GetHashCode}", (float)i.H, minValue: 1) - (float)i.H;

            if (x != 0 || y != 0)
            {
                _propChanged = true;
                cmd.Add(new ItemSpawnMove(i, x, y));
            }

            if (w != 0 || h != 0)
            {
                _propChanged = true;
                cmd.Add(new ItemSpawnResize(i, w, h));
            }
        }

        else
        {
            ImGui.Text("Properties gui not implemented for this object");
        }

        ImGui.End();
    }
}