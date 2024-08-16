using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

partial class PropertiesWindow
{
    public static bool ShowLevelAnimationProps(LevelAnimation la, CommandHistory cmd)
    {
        bool propChanged = false;

        ImGui.Text("FileName: " + la.FileName);
        ImGui.Text($"AnimationName:");
        foreach (string anim in la.AnimationName)
            ImGui.BulletText(anim);
        if (la.PlatID is not null)
            ImGui.Text("PlatID: " + la.PlatID);
        ImGui.Separator();
        // having 3 mutually exclusive properties is very dumb. so give the user a select list.
        LevelAnimationLayerEnum layer = GetLayer(la);
        LevelAnimationLayerEnum newLayer = ImGuiExt.EnumCombo("animation layer", layer);
        if (layer != newLayer)
        {
            cmd.Add(new PropChangeCommand<LevelAnimationLayerEnum>(val => SetLayer(la, val), layer, newLayer));
            propChanged = true;
        }
        // badly named prop. is actually "play even if animated backgrounds are off".
        propChanged |= ImGuiExt.CheckboxHistory("IgnoreOnBlurBG", la.IgnoreOnBlurBG, val => la.IgnoreOnBlurBG = val, cmd);
        // because of how LevelAnimation works, these will only affect the position of the next playing animation
        propChanged |= ImGuiExt.DragDoubleHistory("PositionX", la.PositionX, val => la.PositionX = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("PositionY", la.PositionY, val => la.PositionY = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("RandX", la.RandX, val => la.RandX = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("RandY", la.RandY, val => la.RandY = val, cmd);
        // Scale affect the gfx AnimScale, which requires remaking the texture.
        ImGui.Text("Scale: " + la.Scale);
        // Rotation is in radians. For ease of editing, map it to degrees.
        propChanged |= ImGuiExt.DragDoubleHistory("Rotation", la.Rotation * 180 / Math.PI, val => la.Rotation = BrawlhallaMath.SafeMod(val, 360.0) * Math.PI / 180, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("Flip", la.Flip, val => la.Flip = val, cmd);
        ImGui.Separator();
        // changing these props can desync the timing. don't wanna do that.
        ImGui.Text("InitDelay: " + la.InitDelay);
        ImGui.Text("Interval: " + la.Interval);
        ImGui.Text("IntervalRand: " + la.IntervalRand);
        // this one is fine
        propChanged |= ImGuiExt.DragIntHistory("LoopIterations", la.LoopIterations, val => la.LoopIterations = val, cmd, minValue: 0);

        return propChanged;
    }

    private static LevelAnimationLayerEnum GetLayer(LevelAnimation la)
    {
        return la.PlayMidground
            ? LevelAnimationLayerEnum.MIDGROUND
            : la.PlayBackground
                ? LevelAnimationLayerEnum.BACKGROUND
                : la.PlayForeground
                    ? LevelAnimationLayerEnum.FOREGROUND
                    : LevelAnimationLayerEnum.MIDGROUND;
    }

    private static void SetLayer(LevelAnimation la, LevelAnimationLayerEnum layer)
    {
        switch (layer)
        {
            case LevelAnimationLayerEnum.MIDGROUND:
                la.PlayMidground = true;
                la.PlayBackground = false;
                la.PlayForeground = false;
                break;
            case LevelAnimationLayerEnum.BACKGROUND:
                la.PlayMidground = false;
                la.PlayBackground = true;
                la.PlayForeground = false;
                break;
            case LevelAnimationLayerEnum.FOREGROUND:
                la.PlayMidground = false;
                la.PlayBackground = false;
                la.PlayForeground = true;
                break;
            default:
                break;
        }
    }

    private enum LevelAnimationLayerEnum
    {
        BACKGROUND,
        MIDGROUND,
        FOREGROUND,
    }
}