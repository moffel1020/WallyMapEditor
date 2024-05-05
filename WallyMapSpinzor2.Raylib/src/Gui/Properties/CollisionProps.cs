using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowCollisionProps(AbstractCollision ac, CommandHistory cmd) => ac switch
    {
        AbstractPressurePlateCollision pc => ShowAbstractPressurePlateCollisionProps(pc, cmd),
        _ => ShowAbstractCollisionProps(ac, cmd)
    };

    public static bool ShowAbstractCollisionProps(AbstractCollision ac, CommandHistory cmd)
    {
        bool propChanged = ImGuiExt.DragFloatHistory($"x1##props{ac.GetHashCode()}", ac.X1, (val) => ac.X1 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"y1##props{ac.GetHashCode()}", ac.Y1, (val) => ac.Y1 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"x2##props{ac.GetHashCode()}", ac.X2, (val) => ac.X2 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"y2##props{ac.GetHashCode()}", ac.Y2, (val) => ac.Y2 = val, cmd);

        return propChanged;
    }

    public static bool ShowAbstractPressurePlateCollisionProps(AbstractPressurePlateCollision pc, CommandHistory cmd)
    {
        bool propChanged = ShowAbstractCollisionProps(pc, cmd);
        
        ImGui.SeparatorText("Pressure plate props");
        ImGui.Text("AssetName: " + pc.AssetName);
        propChanged |= ImGuiExt.DragFloatHistory($"anim offset x##props{pc.GetHashCode()}", pc.AnimOffsetX, (val) => pc.AnimOffsetX = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"anim offset y##props{pc.GetHashCode()}", pc.AnimOffsetY, (val) => pc.AnimOffsetY = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"anim rotation##props{pc.GetHashCode()}", pc.AnimRotation, (val) => pc.AnimRotation = BrawlhallaMath.SafeMod(val, 360.0), cmd);
        propChanged |= ImGuiExt.DragIntHistory($"cooldown##props{pc.GetHashCode()}", pc.Cooldown, (val) => pc.Cooldown = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.CheckboxHistory($"facing left##props{pc.GetHashCode()}", pc.FaceLeft, (val) => pc.FaceLeft = val, cmd);
        //TODO: add FireOffsetX, FireOffsetY, TrapPowers

        return propChanged;
    }
}