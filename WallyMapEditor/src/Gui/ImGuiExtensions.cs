using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public static partial class ImGuiExt
{
    private readonly struct DisabledIf_ : IDisposable
    {
        private readonly bool _disabled;

        ///<summary>Do not use this constructor. Use ImGuiExt.DisabledIf instead.</summary>
        internal DisabledIf_(bool disabled)
        {
            _disabled = disabled;
            if (_disabled) ImGui.BeginDisabled();
        }

        void IDisposable.Dispose()
        {
            if (_disabled) ImGui.EndDisabled();
        }
    }
    ///<summary>RAII hack. Should be used as the argument of a using statement.</summary>
    public static IDisposable DisabledIf(bool disabled) => new DisabledIf_(disabled);

    public static string StringListBox(string label, string value, string[] options, float width, int heightItems = 8)
    {
        if (ImGui.BeginListBox(label, new(width, heightItems * ImGui.GetTextLineHeightWithSpacing())))
        {
            int valueIndex = Array.FindIndex(options, s => s == value);
            if (valueIndex == -1) valueIndex = 0;

            foreach (string option in options)
            {
                if (ImGui.Selectable(option, value == option))
                    value = option;
            }
            ImGui.EndListBox();
        }

        return value;
    }

    public static bool ButtonDisabledIf(bool disabled, string label)
    {
        using (DisabledIf(disabled))
            return ImGui.Button(label);
    }

    public static bool MenuItemDisabledIf(bool disabled, string label, string? hotkey = null)
    {
        using (DisabledIf(disabled))
            return ImGui.MenuItem(label, hotkey);
    }

    public static void Animation(RaylibCanvas canvas, Gfx gfx, string animName, long frame)
    {
        Texture2D? texture_ = canvas.Animator.AnimToTexture(gfx, animName, frame);
        if (texture_ is null)
        {
            ImGui.Text("Loading...");
            return;
        }
        Texture2D texture = texture_.Value;
        float ratio = (float)texture.Width / texture.Height;
        // neg height because render texture is flipped vertically
        ImageRect(texture, 128 * ratio, 128, new Rectangle(0, 0, texture.Width, -texture.Height));
    }

    // for some reason, rlImGui exposes the width and height in ImageRect as int, despite the underlying values being float
    // this is a reimplementation without the issue (and is also modified to be simpler)
    public static void ImageRect(Texture2D image, float destWidth, float destHeight, Rectangle sourceRect)
    {
        float uv0X = Math.Sign(sourceRect.Width) * sourceRect.X / image.Width;
        float uv0Y = Math.Sign(sourceRect.Height) * sourceRect.Y / image.Height;
        Vector2 uv0 = new(uv0X, uv0Y);
        float uv1X = uv0X + sourceRect.Width / image.Width;
        float uv1Y = uv0Y + sourceRect.Height / image.Height;
        Vector2 uv1 = new(uv1X, uv1Y);
        ImGui.Image(new IntPtr(image.Id), new Vector2(destWidth, destHeight), uv0, uv1);
    }

    public static void BeginStyledChild(string label)
    {
        unsafe { ImGui.PushStyleColor(ImGuiCol.ChildBg, *ImGui.GetStyleColorVec4(ImGuiCol.FrameBg)); }
        ImGui.BeginChild(label, new Vector2(0, ImGui.GetTextLineHeightWithSpacing() * 8), ImGuiChildFlags.ResizeY | ImGuiChildFlags.Border);
    }

    public static void EndStyledChild()
    {
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    public static bool EditArrayHistory<T>(string label, T[] values, Action<T[]> changeCommand, Func<Maybe<T>> create, Func<int, bool> edit, CommandHistory cmd, bool allowRemove = true, bool allowMove = true)
        where T : notnull
    {
        List<(PropChangeCommand<T[]>, bool)> commands = [];
        BeginStyledChild(label);
        bool changed = false;
        for (int i = 0; i < values.Length; ++i)
        {
            T value = values[i];
            changed |= edit(i);
            if (ButtonDisabledIf(!allowRemove, $"x##{i}{value.GetHashCode()}"))
            {
                T[] result = WmeUtils.RemoveAt(values, i);
                commands.Add((new ArrayRemoveCommand<T>(changeCommand, values, result, value), false));
                changed = true;
            }
            if (allowMove)
            {
                ImGui.SameLine();
                if (ButtonDisabledIf(i == 0, $"^##{i}{value.GetHashCode()}"))
                {
                    T[] result = WmeUtils.MoveUp(values, i);
                    commands.Add((new PropChangeCommand<T[]>(changeCommand, values, result), false));
                    changed = true;
                }
                ImGui.SameLine();
                if (ButtonDisabledIf(i == values.Length - 1, $"v##{i}{value.GetHashCode()}"))
                {
                    T[] result = WmeUtils.MoveDown(values, i);
                    commands.Add((new PropChangeCommand<T[]>(changeCommand, values, result), false));
                    changed = true;
                }
            }
        }
        EndStyledChild();
        Maybe<T> maybeNewValue = create();
        if (maybeNewValue.TryGetValue(out T? newValue))
        {
            commands.Add((new ArrayAddCommand<T>(changeCommand, values, newValue), false));
            changed = true;
        }

        foreach ((ICommand command, bool mergeable) in commands)
            cmd.Add(command, mergeable);

        return changed;
    }

    public static void HeaderWithWidget(string label, Action headerAction, Action widget, int rightOffset = 15)
    {
        ImGui.SetNextItemAllowOverlap();
        bool header = ImGui.CollapsingHeader(label);
        ImGui.SameLine(ImGui.GetContentRegionMax().X - rightOffset);
        widget();
        if (header) headerAction();
    }

    public static void HintTooltip(string explanation)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(explanation);
    }

    public static Vector4 RGBHexToVec4(uint hex)
    {
        float r = ((hex >> 16) & 0xFF) / 255f;
        float g = ((hex >> 8) & 0xFF) / 255f;
        float b = (hex & 0xFF) / 255f;
        return new(r, g, b, 1);
    }
}