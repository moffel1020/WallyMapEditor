using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowKeyFrameProps(KeyFrame k, CommandHistory cmd, int minFrameNum = 0, int maxFrameNum = int.MaxValue)
    {
        bool propChanged = false;

        propChanged |= ImGuiExt.DragIntHistory("FrameNum", k.FrameNum, val => k.FrameNum = val, cmd, minValue: minFrameNum, maxValue: maxFrameNum);

        propChanged |= ImGuiExt.DragDoubleHistory("X", k.X, val => k.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", k.Y, val => k.Y = val, cmd);
        // not implemented in the renderer yet. also stored as radians for some reason.
        //propChanged |= ImGuiExt.DragDoubleHistory("Rotation", k.Rotation, val => k.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.2f);

        propChanged |= ImGuiExt.DragNullableDoubleHistory("CenterX", k.CenterX, 0, val => k.CenterX = val, cmd);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("CenterY", k.CenterY, 0, val => k.CenterY = val, cmd);
        ImGui.Spacing();
        propChanged |= ImGuiExt.NullableCheckboxHistory("EaseIn", k.EaseIn, false, val => k.EaseIn = val, cmd);
        propChanged |= ImGuiExt.NullableCheckboxHistory("EaseOut", k.EaseOut, false, val => k.EaseOut = val, cmd);
        propChanged |= ImGuiExt.DragNullableIntHistory("EasePower", k.EasePower, 2, val => k.EasePower = val, cmd, minValue: 2);

        return propChanged;
    }

    public static bool ShowOneOfManyKeyFrameProps(AbstractKeyFrame[] frames, int index, CommandHistory cmd)
    {
        bool propChanged = false;
        AbstractKeyFrame key = frames[index];
        if (key is KeyFrame keyFrame && ImGui.TreeNode($"KeyFrame {MapOverviewWindow.GetExtraObjectInfo(key)}###akf{key.GetHashCode()}"))
        {
            int minFrameNum = 0;
            int maxFrameNum = int.MaxValue;
            if (index - 1 >= 0) minFrameNum = LastKeyFrameNum(frames[index - 1]) + 1;
            if (index + 1 < frames.Length) maxFrameNum = FirstKeyFrameNum(frames[index + 1]) - 1;

            propChanged |= ShowKeyFrameProps(keyFrame, cmd, minFrameNum, maxFrameNum);
            ImGui.TreePop();
        }
        else if (key is Phase phase && ImGui.TreeNode($"Phase {MapOverviewWindow.GetExtraObjectInfo(key)}###akf{key.GetHashCode()}"))
        {
            int minStartFrame = 0;
            int maxFrameNum = int.MaxValue;
            if (index - 1 >= 0) minStartFrame = LastKeyFrameNum(frames[index - 1]) + 1;
            if (index + 1 < frames.Length) maxFrameNum = FirstKeyFrameNum(frames[index + 1]) - 1;

            propChanged |= ShowPhaseProps(phase, cmd, minStartFrame, maxFrameNum);
            ImGui.TreePop();
        }

        return propChanged;
    }

    public static int LastKeyFrameNum(AbstractKeyFrame akf) => akf switch
    {
        KeyFrame kf => kf.FrameNum,
        Phase p => p.StartFrame + LastKeyFrameNum(p.KeyFrames),
        _ => throw new ArgumentException($"Unknown keyframe type {akf.GetType().Name}")
    };

    public static int FirstKeyFrameNum(AbstractKeyFrame akf) => akf switch
    {
        KeyFrame kf => kf.FrameNum,
        Phase p => p.StartFrame,
        _ => throw new ArgumentException($"Unknown keyframe type {akf.GetType().Name}")
    };

    public static KeyFrame DefaultKeyFrame(int lastKeyFrameNum) => new()
    {
        FrameNum = lastKeyFrameNum + 1,
    };
}