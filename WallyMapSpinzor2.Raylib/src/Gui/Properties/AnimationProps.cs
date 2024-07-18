using System;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowAnimationProps(Animation anim, CommandHistory cmd)
    {
        bool propChanged = false;

        propChanged |= ImGuiExt.DragNullableIntHistory("NumFrames", anim.NumFrames, LastKeyFrameNum(anim.KeyFrames), val => anim.NumFrames = val, cmd, minValue: LastKeyFrameNum(anim.KeyFrames));
        propChanged |= ImGuiExt.DragNullableFloatHistory("SlowMult", anim.SlowMult, 1, val => anim.SlowMult = val, cmd, speed: 0.05);
        propChanged |= ImGuiExt.DragIntHistory("StartFrame", anim.StartFrame, val => anim.StartFrame = val, cmd, minValue: 0, maxValue: anim.NumFrames ?? int.MaxValue);
        propChanged |= ImGuiExt.CheckboxHistory("EaseIn", anim.EaseIn, val => anim.EaseIn = val, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("EaseOut", anim.EaseOut, val => anim.EaseOut = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("EasePower", anim.EasePower, val => anim.EasePower = val, cmd, minValue: 2);

        if (anim.HasCenter)
        {
            propChanged |= ImGuiExt.DragFloatHistory("CenterX", anim.CenterX!.Value, val => anim.CenterX = val, cmd);
            propChanged |= ImGuiExt.DragFloatHistory("CenterY", anim.CenterY!.Value, val => anim.CenterY = val, cmd);
            if (ImGui.Button("Remove center"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<(double?, double?)>(
                    val => (anim.CenterX, anim.CenterY) = val,
                    (anim.CenterX, anim.CenterY),
                    (null, null)));
            }
        }
        else if (ImGui.Button("Add center"))
        {
            propChanged = true;
            cmd.Add(new PropChangeCommand<(double?, double?)>(
                val => (anim.CenterX, anim.CenterY) = val,
                (anim.CenterX, anim.CenterY),
                (0, 0)));
        }

        if (ImGui.CollapsingHeader("KeyFrames"))
        {
            propChanged |=
            ImGuiExt.EditArrayHistory("##animationFrames", anim.KeyFrames, val => anim.KeyFrames = val,
            // create
            () => CreateKeyFrame(LastKeyFrameNum(anim.KeyFrames)),
            // edit
            (int index) =>
            {
                if (index != 0)
                    ImGui.Separator();
                return ShowOneOfManyKeyFrameProps(anim.KeyFrames, index, cmd);
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