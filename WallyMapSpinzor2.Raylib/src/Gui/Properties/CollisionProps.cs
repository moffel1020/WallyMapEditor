using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowCollisionProps(AbstractCollision ac, CommandHistory cmd) => ac switch
    {
        AbstractPressurePlateCollision pc => ShowAbstractPressurePlateCollisionProps(pc, cmd),
        LavaCollision lc => ShowLavaCollisionProps(lc, cmd),
        _ => ShowAbstractCollisionProps(ac, cmd)
    };

    public static bool ShowAbstractCollisionProps(AbstractCollision ac, CommandHistory cmd)
    {
        bool propChanged = ImGuiExt.DragFloatHistory($"x1##props{ac.GetHashCode()}", ac.X1, val => ac.X1 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"y1##props{ac.GetHashCode()}", ac.Y1, val => ac.Y1 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"x2##props{ac.GetHashCode()}", ac.X2, val => ac.X2 = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"y2##props{ac.GetHashCode()}", ac.Y2, val => ac.Y2 = val, cmd);

        ImGui.SeparatorText($"Anchor##props{ac.GetHashCode()}");
        propChanged |= ImGuiExt.DragNullableFloatPairHistory(
            "anchor",
            $"anchor x##props{ac.GetHashCode()}", $"anchor y##props{ac.GetHashCode()}",
            ac.AnchorX, ac.AnchorY,
            // these default values look weird when the collision is on a moving platform
            // but the user will adjust it anyways so it's ok
            (ac.X1 + ac.X2) / 2, (ac.Y1 + ac.Y2) / 2,
            (val1, val2) => (ac.AnchorX, ac.AnchorY) = (val1, val2),
            cmd
        );

        ImGui.SeparatorText($"Normal##props{ac.GetHashCode()}");
        propChanged |= ImGuiExt.DragFloatHistory($"normal x##props{ac.GetHashCode()}", ac.NormalX, val => ac.NormalX = val, cmd, speed: 0.01, minValue: -1, maxValue: 1);
        propChanged |= ImGuiExt.DragFloatHistory($"normal y##props{ac.GetHashCode()}", ac.NormalY, val => ac.NormalY = val, cmd, speed: 0.01, minValue: -1, maxValue: 1);

        return propChanged;
    }

    public static bool ShowAbstractPressurePlateCollisionProps(AbstractPressurePlateCollision pc, CommandHistory cmd)
    {
        bool propChanged = ShowAbstractCollisionProps(pc, cmd);

        ImGui.SeparatorText($"Pressure plate props##props{pc.GetHashCode()}");
        ImGui.Text("AssetName: " + pc.AssetName); //TODO: allow modifying
        propChanged |= ImGuiExt.DragFloatHistory($"anim offset x##props{pc.GetHashCode()}", pc.AnimOffsetX, val => pc.AnimOffsetX = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"anim offset y##props{pc.GetHashCode()}", pc.AnimOffsetY, val => pc.AnimOffsetY = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"anim rotation##props{pc.GetHashCode()}", pc.AnimRotation, val => pc.AnimRotation = BrawlhallaMath.SafeMod(val, 360.0), cmd);
        propChanged |= ImGuiExt.DragIntHistory($"cooldown##props{pc.GetHashCode()}", pc.Cooldown, val => pc.Cooldown = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.CheckboxHistory($"facing left##props{pc.GetHashCode()}", pc.FaceLeft, val => pc.FaceLeft = val, cmd);
        //TODO: add FireOffsetX, FireOffsetY

        //TODO: allow modifying
        if (ImGui.BeginListBox($"powers##props{pc.GetHashCode()}"))
        {
            foreach (string power in pc.TrapPowers)
                ImGui.Text(power);
            ImGui.EndListBox();
        }

        return propChanged;
    }

    public static bool ShowLavaCollisionProps(LavaCollision lc, CommandHistory cmd)
    {
        bool propChanged = ShowAbstractCollisionProps(lc, cmd);

        ImGui.SeparatorText($"Lava collision props##props{lc.GetHashCode()}");
        ImGui.Text("LavaPower: " + lc.LavaPower); //TODO: allow modifying

        return propChanged;
    }
}