using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowPhaseProps(Phase phase, EditorLevel level, int minStartFrame = 0, int maxFrameNum = int.MaxValue)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;
        propChanged |= ImGuiExt.DragIntHistory("StartFrame", phase.StartFrame, val => phase.StartFrame = val, cmd, minValue: minStartFrame, maxValue: maxFrameNum);
        propChanged |= ImGuiExt.EditArrayHistory($"##phaseFrames{phase.GetHashCode()}", phase.KeyFrames, val => phase.KeyFrames = val,
            // create
            () => CreateKeyFrame(LastKeyFrameNum(phase.KeyFrames), phase),
            // edit
            index => ShowOneOfManyKeyFrameProps(phase.KeyFrames, index, level),
            cmd, allowMove: false);

        return propChanged;
    }

    public static Phase DefaultPhase(int lastKeyFrameNum) => new()
    {
        StartFrame = lastKeyFrameNum + 1,
        KeyFrames = [],
    };
}