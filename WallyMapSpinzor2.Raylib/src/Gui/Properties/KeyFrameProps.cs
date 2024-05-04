using System;
using System.Collections.Generic;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowKeyFrameProps(KeyFrame k, CommandHistory cmd, int minFrameNum = 0, int maxFrameNum = int.MaxValue)
    {
        bool propChanged = false;

        propChanged |= ImGuiExt.DragIntHistory("FrameNum", k.FrameNum, (val) => k.FrameNum = val, cmd, minValue: minFrameNum, maxValue: maxFrameNum);        

        propChanged |= ImGuiExt.DragFloatHistory("x", k.X, (val) => k.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("y", k.Y, (val) => k.Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Rotation", k.Rotation, (val) => k.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.2);

        if (k.HasCenter)
        {
            propChanged |= ImGuiExt.DragFloatHistory("CenterX", (double)k.CenterX!, (val) => k.CenterX = val, cmd);
            propChanged |= ImGuiExt.DragFloatHistory("CenterY", (double)k.CenterY!, (val) => k.CenterY = val, cmd);
            if (ImGui.Button("Remove center"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<(double?, double?)>(
                    (val) => (k.CenterX, k.CenterY) = val, 
                    (k.CenterX, k.CenterY),
                    (null, null)));
            }
        }
        else if (ImGui.Button("Add center"))
        {
            propChanged = true;
            cmd.Add(new PropChangeCommand<(double?, double?)>(
                (val) => (k.CenterX, k.CenterY) = val, 
                (k.CenterX, k.CenterY),
                (0, 0)));
        }

        propChanged |= ImGuiExt.CheckboxHistory("EaseIn", k.EaseIn, (val) => k.EaseIn = val, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("EaseOut", k.EaseOut, (val) => k.EaseOut = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("EasePower", k.EasePower, (val) => k.EasePower = val, cmd);

        return propChanged;
    }

    public static bool ShowManyKeyFrameProps(List<AbstractKeyFrame> frames, CommandHistory cmd)
    {
        bool propChanged = false;

        for (int i = 0; i < frames.Count; i++)
        {
            AbstractKeyFrame kf = frames[i];
            if (kf is KeyFrame keyFrame && ImGui.TreeNode($"KeyFrame {MapOverviewWindow.GetExtraObjectInfo(kf)}###akf{kf.GetHashCode()}"))
            {
                int minFrameNum = 0;
                int maxFrameNum = int.MaxValue;
                if (i - 1 >= 0) minFrameNum = LastKeyFrameNum(frames[i - 1]) + 1;
                if (i + 1 < frames.Count) maxFrameNum = FirstKeyFrameNum(frames[i + 1]) - 1;

                propChanged |= ShowKeyFrameProps(keyFrame, cmd, minFrameNum, maxFrameNum);
                ImGui.TreePop();
            }
            else if (kf is Phase phase && ImGui.TreeNode($"Phase###akf{kf.GetHashCode}"))
            {
                int minStartFrame = 0;
                int maxFrameNum = int.MaxValue;
                if (i - 1 >= 0) minStartFrame = LastKeyFrameNum(frames[i - 1]);
                if (i + 1 < frames.Count) maxFrameNum = FirstKeyFrameNum(frames[i + 1]);

                propChanged |= ShowPhaseProps(phase, cmd, minStartFrame, maxFrameNum);
                ImGui.TreePop();
            }
        }

        return propChanged;
    }

    public static int LastKeyFrameNum(AbstractKeyFrame akf) => akf switch
    {
        KeyFrame kf => kf.FrameNum,
        Phase p => LastKeyFrameNum(p.KeyFrames),
        _ => throw new InvalidOperationException("Could not find the last keyframenum. type of abstract keyframe type is not implemented")
    };

    public static int FirstKeyFrameNum(AbstractKeyFrame akf) => akf switch
    {
        KeyFrame kf => kf.FrameNum,
        Phase p => p.StartFrame,
        _ => throw new InvalidOperationException("Could not find the first keyframenum. type of abstract keyframe type is not implemented")
    };
}