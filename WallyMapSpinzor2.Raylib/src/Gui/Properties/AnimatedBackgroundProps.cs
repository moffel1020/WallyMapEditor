using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowAnimatedBackgroundProps(AnimatedBackground ab, CommandHistory cmd)
    {
        bool propChanged = false;
        if (ImGui.CollapsingHeader("Gfx"))
            propChanged |= ShowGfxProps(ab.Gfx, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("Midground", ab.Midground, val => ab.Midground = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("PositionX", ab.Position_X, val => ab.Position_X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("PositionY", ab.Position_Y, val => ab.Position_Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("ScaleX", ab.Scale_X, val => ab.Scale_X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("ScaleY", ab.Scale_Y, val => ab.Scale_Y = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("SkewX", ab.Skew_X, val => ab.Skew_X = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1);
        propChanged |= ImGuiExt.DragFloatHistory("SkewY", ab.Skew_Y, val => ab.Skew_Y = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1);
        propChanged |= ImGuiExt.DragFloatHistory("Rotation", ab.Rotation, val => ab.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1);
        propChanged |= ImGuiExt.DragIntHistory("FrameOffset", ab.FrameOffset, val => ab.FrameOffset = val, cmd);
        propChanged |= ImGuiExt.CheckboxHistory("ForceDraw", ab.ForceDraw, val => ab.ForceDraw = val, cmd);
        return propChanged;
    }

    // unused variable warning
#pragma warning disable IDE0060
    public static bool ShowGfxProps(Gfx g, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("AnimFile: " + g.AnimFile);
        ImGui.Text("AnimClass: " + g.AnimClass);
        // changing AnimScale requires remaking the texture
        // we're not gonna do that, so don't let users just edit it
        ImGui.Text("AnimScale: " + g.AnimScale);
        return propChanged;
    }
#pragma warning restore IDE0060
}