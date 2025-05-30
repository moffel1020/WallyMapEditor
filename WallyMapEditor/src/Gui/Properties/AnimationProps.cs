using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowAnimationProps(Animation anim, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;

        propChanged |= ImGuiExt.DragNullableIntHistory("NumFrames", anim.NumFrames, LastKeyFrameNum(anim.KeyFrames), val => anim.NumFrames = val, cmd, minValue: LastKeyFrameNum(anim.KeyFrames));
        propChanged |= ImGuiExt.DragNullableDoubleHistory("SlowMult", anim.SlowMult, 1, val => anim.SlowMult = val, cmd, speed: 0.05f);
        propChanged |= ImGuiExt.DragUIntHistory("StartFrame", anim.StartFrame, val => anim.StartFrame = val, cmd, maxValue: (uint?)anim.NumFrames ?? uint.MaxValue);

        ImGui.SeparatorText("Easing");
        ImGuiExt.HintTooltip(Strings.UI_EASING_TOOLTIP);
        propChanged |= ImGuiExt.CheckboxHistory("EaseIn", anim.EaseIn, val => anim.EaseIn = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_EASE_IN_TOOLTIP);
        propChanged |= ImGuiExt.CheckboxHistory("EaseOut", anim.EaseOut, val => anim.EaseOut = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_EASE_OUT_TOOLTIP);
        using (ImGuiExt.DisabledIf(!anim.EaseIn && !anim.EaseOut))
            propChanged |= ImGuiExt.DragUIntHistory("EasePower", anim.EasePower, val => anim.EasePower = val, cmd, minValue: 2);
        ImGuiExt.HintTooltip(Strings.UI_EASE_POWER_TOOLTIP);

        ImGui.SeparatorText("Center");
        ImGuiExt.HintTooltip(Strings.UI_CENTER_TOOLTIP);
        propChanged |= ImGuiExt.DragNullableDoublePairHistory(
            "center",
            "CenterX", "CenterY",
            anim.CenterX, anim.CenterY,
            0, 0,
            (val1, val2) => (anim.CenterX, anim.CenterY) = (val1, val2),
            cmd
        );

        ImGui.Separator();
        if (ImGui.CollapsingHeader("KeyFrames"))
        {
            propChanged |=
            ImGuiExt.EditArrayHistory("##animationFrames", anim.KeyFrames, val => anim.KeyFrames = val,
            // create
            () => CreateKeyFrame(LastKeyFrameNum(anim.KeyFrames)),
            // edit
            index =>
            {
                if (index != 0)
                    ImGui.Separator();
                return ShowOneOfManyKeyFrameProps(anim.KeyFrames, index, level);
            },
            cmd, allowRemove: anim.KeyFrames.Length > 1, allowMove: false);
        }

        return propChanged;
    }

    public static int LastKeyFrameNum(AbstractKeyFrame[] keyFrames) => keyFrames.Length == 0
        ? 0
        : keyFrames[^1] switch
        {
            KeyFrame kf => kf.FrameNum,
            Phase p => p.StartFrame + LastKeyFrameNum(p.KeyFrames),
            _ => throw new ArgumentException($"Unknown keyframe type {keyFrames[^1].GetType().Name}")
        };

    public static Maybe<AbstractKeyFrame> CreateKeyFrame(int lastKeyFrameNum, Phase? parent = null)
    {
        Maybe<AbstractKeyFrame> result = new();
        if (ImGui.Button("Add new keyframe"))
            ImGui.OpenPopup("AddKeyframe");

        if (ImGui.BeginPopup("AddKeyframe"))
        {
            if (ImGui.MenuItem("KeyFrame"))
                result = DefaultKeyFrame(lastKeyFrameNum);
            if (ImGui.MenuItem("Phase"))
                result = DefaultPhase(lastKeyFrameNum);

            if (parent is not null)
                result.DoIfSome(k => k.Parent = parent);

            ImGui.EndPopup();
        }
        return result;
    }
}