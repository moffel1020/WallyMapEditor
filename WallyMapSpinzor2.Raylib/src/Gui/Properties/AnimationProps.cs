using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowAnimationProps(Animation anim, CommandHistory cmd)
    {
        bool propChanged = false;

        if (anim.NumFrames is null)
        {
            ImGui.Text("NumFrames");
            ImGui.SameLine();
            if (ImGui.Button("Add##numframes"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<int?>((val) => anim.NumFrames = val, anim.NumFrames, LastKeyFrameNum(anim.KeyFrames)));
            }
        }
        else
        {
            propChanged |= ImGuiExt.DragIntHistory("NumFrames", anim.NumFrames!.Value, (val) => anim.NumFrames = val, cmd, minValue: LastKeyFrameNum(anim.KeyFrames));
            ImGui.SameLine();
            if (ImGui.Button("Remove##numframes"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<int?>((val) => anim.NumFrames = val, anim.NumFrames, null));
            }
        }

        if (anim.SlowMult is null)
        {
            ImGui.Text("SlowMult");
            ImGui.SameLine();
            if (ImGui.Button("Add##slowmult"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<double?>((val) => anim.SlowMult = val, anim.SlowMult, 1));
            }
        }
        else
        {
            propChanged |= ImGuiExt.DragFloatHistory("SlowMult", anim.SlowMult!.Value, (val) => anim.SlowMult = val, cmd, speed: 0.05);
            ImGui.SameLine();
            if (ImGui.Button("Remove##slowmult"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<double?>((val) => anim.SlowMult = val, anim.SlowMult, null));
            }
        }

        propChanged |= ImGuiExt.DragIntHistory("Start frame", anim.StartFrame, (val) => anim.StartFrame = val, cmd, minValue: 0, maxValue: anim.NumFrames ?? int.MaxValue); // FIXME: probably needs to be max of leveldesc if not present here
        propChanged |= ImGuiExt.CheckboxHistory("Ease in", anim.EaseIn, (val) => anim.EaseIn = val, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("Ease out", anim.EaseOut, (val) => anim.EaseOut = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("Ease power", anim.EasePower, (val) => anim.EasePower = val, cmd, minValue: 0);

        if (anim.HasCenter)
        {
            propChanged |= ImGuiExt.DragFloatHistory("CenterX", anim.CenterX!.Value, (val) => anim.CenterX = val, cmd);
            propChanged |= ImGuiExt.DragFloatHistory("CenterY", anim.CenterY!.Value, (val) => anim.CenterY = val, cmd);
            if (ImGui.Button("Remove center"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<(double?, double?)>(
                    (val) => (anim.CenterX, anim.CenterY) = val,
                    (anim.CenterX, anim.CenterY),
                    (null, null)));
            }
        }
        else if (ImGui.Button("Add center"))
        {
            propChanged = true;
            cmd.Add(new PropChangeCommand<(double?, double?)>(
                (val) => (anim.CenterX, anim.CenterY) = val,
                (anim.CenterX, anim.CenterY),
                (0, 0)));
        }

        if (ImGui.CollapsingHeader("KeyFrames"))
        {
            propChanged |= ShowManyKeyFrameProps(anim.KeyFrames, cmd);
        }

        return propChanged;
    }

    public static int LastKeyFrameNum(IEnumerable<AbstractKeyFrame> keyFrames) => keyFrames.Last() switch
    {
        KeyFrame kf => kf.FrameNum,
        Phase p => LastKeyFrameNum(p.KeyFrames),
        _ => throw new InvalidOperationException("Could not find the last keyframenum. type of abstract keyframe type is not implemented")
    };

}