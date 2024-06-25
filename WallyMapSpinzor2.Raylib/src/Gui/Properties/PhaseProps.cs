namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowPhaseProps(Phase phase, CommandHistory cmd, int minStartFrame = 0, int maxFrameNum = int.MaxValue)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragIntHistory("StartFrame", phase.StartFrame, val => phase.StartFrame = val, cmd, minValue: minStartFrame, maxValue: maxFrameNum);
        propChanged |= ImGuiExt.EditArrayHistory("", phase.KeyFrames, val => phase.KeyFrames = val,
            // create
            () => CreateKeyFrame(LastKeyFrameNum(phase.KeyFrames), phase),
            // edit
            (int index) => ShowOneOfManyKeyFrameProps(phase.KeyFrames, index, cmd),
            cmd, allowMove: false);

        return propChanged;
    }

    public static Phase DefaultPhase(int lastKeyFrameNum) => new()
    {
        StartFrame = lastKeyFrameNum + 1,
        KeyFrames = [],
    };
}