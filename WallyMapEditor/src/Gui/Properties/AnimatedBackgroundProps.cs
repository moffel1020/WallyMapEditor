using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

partial class PropertiesWindow
{
    public static bool ShowAnimatedBackgroundProps(AnimatedBackground ab, CommandHistory cmd, PropertiesWindowData data)
    {
        if (data.Level is not null)
            RemoveButton(ab, data.Level.Desc, cmd);
        ImGui.Separator();

        bool propChanged = false;

        if (ImGui.CollapsingHeader("Gfx"))
            propChanged |= ShowProperties(ab.Gfx, cmd, data);

        ImGui.SeparatorText("Display");
        propChanged |= ImGuiExt.CheckboxHistory("Midground", ab.Midground, val => ab.Midground = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_ANIMATED_MIDGROUND_TOOLTIP);
        propChanged |= ImGuiExt.CheckboxHistory("ForceDraw", ab.ForceDraw, val => ab.ForceDraw = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_FORCE_DRAW_TOOLTIP);

        ImGui.SeparatorText("Transform");
        propChanged |= ImGuiExt.DragDoubleHistory("PositionX", ab.Position_X, val => ab.Position_X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("PositionY", ab.Position_Y, val => ab.Position_Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("ScaleX", ab.Scale_X, val => ab.Scale_X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("ScaleY", ab.Scale_Y, val => ab.Scale_Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("SkewX", ab.Skew_X, val => ab.Skew_X = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1f);
        propChanged |= ImGuiExt.DragDoubleHistory("SkewY", ab.Skew_Y, val => ab.Skew_Y = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1f);
        propChanged |= ImGuiExt.DragDoubleHistory("Rotation", ab.Rotation, val => ab.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1f);

        ImGui.SeparatorText("Animation");
        propChanged |= ImGuiExt.DragIntHistory("FrameOffset", ab.FrameOffset, val => ab.FrameOffset = val, cmd);
        // do nullable editing because 0 = infinity
        propChanged |= ImGuiExt.DragNullableUIntHistory("Loops", ab.Loops == 0 ? null : ab.Loops, 1, val => ab.Loops = val ?? 0, cmd, minValue: 1);

        ImGui.SeparatorText("Sound");
        if (ab.SoundString is not null)
        {
            propChanged |= ImGuiExt.InputTextHistory("SoundString", ab.SoundString, val => ab.SoundString = val, cmd);
            propChanged |= ImGuiExt.DragUIntHistory("SoundFrame", ab.SoundFrame ?? 0, val => ab.SoundFrame = val, cmd);
            if (ImGui.Button("Remove##SoundString"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<string?, uint?>(
                    (val1, val2) =>
                    {
                        ab.SoundString = val1;
                        ab.SoundFrame = val2;
                    },
                    ab.SoundString, ab.SoundFrame,
                    null, null
                ), false);
            }
        }
        else
        {
            if (ImGui.Button("Add##SoundString"))
            {
                propChanged = true;
                cmd.Add(new PropChangeCommand<string?, uint?>(
                    (val1, val2) =>
                    {
                        ab.SoundString = val1;
                        ab.SoundFrame = val2;
                    },
                    ab.SoundString, ab.SoundFrame,
                    "", ab.SoundFrame ?? 0
                ), false);
            }
        }

        return propChanged;
    }

    // Remove unused parameter 'cmd' if it is not part of a shipped public API [WallyMapEditor]
#pragma warning disable IDE0060
    public static bool ShowGfxProps(Gfx g, CommandHistory cmd, PropertiesWindowData data)
#pragma warning restore IDE0060
    {
        bool propChanged = false;
        ImGui.Text("AnimFile: " + g.AnimFile);
        ImGui.Text("AnimClass: " + g.AnimClass);
        if (data.Canvas is not null)
        {
            ImGui.Spacing();
            ImGui.Indent();
            ImGuiExt.Animation(data.Canvas, g, "Ready", LevelDesc.GET_ANIM_FRAME(data.Time));
            ImGui.Unindent();
            ImGui.Spacing();
        }
        // changing AnimScale requires remaking the texture
        // we're not gonna do that, so don't let users just edit it
        ImGui.Text("AnimScale: " + g.AnimScale);
        return propChanged;
    }
}