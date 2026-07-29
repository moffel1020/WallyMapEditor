using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowLevelMudProps(LevelMud mud, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;
        propChanged |= ImGuiExt.DragDoubleHistory("MudY", mud.MudY, val => mud.MudY = val, cmd, maxValue: 9998);
        propChanged |= ImGuiExt.DragDoubleHistory("MudFallMult", mud.MudFallMult, val => mud.MudFallMult = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragDoubleHistory("MudFallStunMult", mud.MudFallStunMult, val => mud.MudFallStunMult = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragDoubleHistory("MudXSpeedMult", mud.MudXSpeedMult, val => mud.MudXSpeedMult = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragDoubleHistory("MudKillDepth", mud.MudKillDepth, val => mud.MudKillDepth = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.CheckboxHistory("MudJumpBack", mud.MudJumpBack, val => mud.MudJumpBack = val, cmd);

        return propChanged;
    }

    public static LevelMud DefaultMud => new()
    {
        MudY = 2450,
    };
}