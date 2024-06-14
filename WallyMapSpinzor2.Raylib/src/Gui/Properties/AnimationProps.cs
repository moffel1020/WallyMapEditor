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
            ImGuiExt.EditArrayHistory("", anim.KeyFrames, val => anim.KeyFrames = val,
            // create
            () => CreateKeyFrame(anim),
            // edit
            (int index) =>
            {
                if (index != 0)
                    ImGui.Separator();
                propChanged |= ShowAnimationKeyFrameProps(anim, index, cmd);
            },
            cmd, allowMove: false);
        }

        return propChanged;
    }

    public static int LastKeyFrameNum(AbstractKeyFrame[] keyFrames) => keyFrames.Length == 0
        ? 0
        : keyFrames[^1] switch
        {
            KeyFrame kf => kf.FrameNum,
            Phase p => p.StartFrame + LastKeyFrameNum(p.KeyFrames),
            _ => throw new InvalidOperationException("Could not find the last keyframenum. type of abstract keyframe type is not implemented")
        };

    private static Maybe<AbstractKeyFrame> CreateKeyFrame(Animation anim)
    {
        Maybe<AbstractKeyFrame> result = new();
        if (ImGui.BeginMenu("Add new keyframe"))
        {
            if (ImGui.MenuItem("KeyFrame"))
            {
                result = DefaultKeyFrame(anim);
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.MenuItem("Phase"))
            {
                result = DefaultPhase(anim);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndMenu();
        }
        return result;
    }
}