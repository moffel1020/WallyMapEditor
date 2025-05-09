using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowKeyFrameProps(KeyFrame k, EditorLevel level, int minFrameNum = 0, int maxFrameNum = int.MaxValue)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;

        propChanged |= ImGuiExt.DragIntHistory("FrameNum", k.FrameNum, val => k.FrameNum = val, cmd, minValue: minFrameNum, maxValue: maxFrameNum);

        propChanged |= ImGuiExt.DragDoubleHistory("X", k.X, val => k.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", k.Y, val => k.Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Rotation", k.Rotation, val => k.Rotation = val, cmd, speed: 0.1f);

        ImGui.SeparatorText("Easing");
        ImGuiExt.HintTooltip(Strings.UI_EASING_TOOLTIP);
        propChanged |= ImGuiExt.NullableCheckboxHistory("EaseIn", k.EaseIn, false, val => k.EaseIn = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_EASE_IN_TOOLTIP);
        propChanged |= ImGuiExt.NullableCheckboxHistory("EaseOut", k.EaseOut, false, val => k.EaseOut = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_EASE_OUT_TOOLTIP);
        using (ImGuiExt.DisabledIf(k.EaseIn != true && k.EaseOut != true))
            propChanged |= ImGuiExt.DragUIntHistory("EasePower", k.EasePower ?? 2, val => k.EasePower = val, cmd, minValue: 2);
        ImGuiExt.HintTooltip(Strings.UI_EASE_POWER_TOOLTIP);

        ImGui.SeparatorText("Center");
        ImGuiExt.HintTooltip(Strings.UI_CENTER_TOOLTIP);
        propChanged |= ImGuiExt.DragNullableDoublePairHistory(
            "center",
            "CenterX", "CenterY",
            k.CenterX, k.CenterY,
            0, 0,
            (val1, val2) => (k.CenterX, k.CenterY) = (val1, val2),
            cmd
        );

        ImGui.SeparatorText("Respawning");
        propChanged |= ImGuiExt.CheckboxHistory("RespawnOff", k.RespawnOff, val => k.RespawnOff = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_RESPAWN_OFF_TOOLTIP);

        return propChanged;
    }

    public static bool ShowOneOfManyKeyFrameProps(AbstractKeyFrame[] frames, int index, EditorLevel level)
    {
        bool propChanged = false;
        AbstractKeyFrame key = frames[index];
        if (key is KeyFrame keyFrame && ImGui.TreeNode($"KeyFrame {MapOverviewWindow.GetExtraObjectInfo(key)}###akf{key.GetHashCode()}"))
        {
            int minFrameNum = 0;
            int maxFrameNum = int.MaxValue;
            if (index - 1 >= 0) minFrameNum = LastKeyFrameNum(frames[index - 1]) + 1;
            if (index + 1 < frames.Length) maxFrameNum = FirstKeyFrameNum(frames[index + 1]) - 1;

            propChanged |= ShowKeyFrameProps(keyFrame, level, minFrameNum, maxFrameNum);
            ImGui.TreePop();
        }
        else if (key is Phase phase && ImGui.TreeNode($"Phase {MapOverviewWindow.GetExtraObjectInfo(key)}###akf{key.GetHashCode()}"))
        {
            int minStartFrame = 0;
            int maxFrameNum = int.MaxValue;
            if (index - 1 >= 0) minStartFrame = LastKeyFrameNum(frames[index - 1]) + 1;
            if (index + 1 < frames.Length) maxFrameNum = FirstKeyFrameNum(frames[index + 1]) - 1;

            propChanged |= ShowPhaseProps(phase, level, minStartFrame, maxFrameNum);
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