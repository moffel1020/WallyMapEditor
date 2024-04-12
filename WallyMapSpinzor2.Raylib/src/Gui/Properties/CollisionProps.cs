namespace  WallyMapSpinzor2.Raylib;
using WallyMapSpinzor2;

public partial class PropertiesWindow
{
    public bool ShowAbstractCollisionProps(AbstractCollision ac, CommandHistory cmd)
    {
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

        return true;
    }
}