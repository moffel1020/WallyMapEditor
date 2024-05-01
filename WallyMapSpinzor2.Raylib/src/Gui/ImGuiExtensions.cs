using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public static class ImGuiExt
{
    public static bool Checkbox(string label, bool value)
    {
        ImGui.Checkbox(label, ref value);
        return value;
    }

    public static int DragInt(string label, int value, int speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        ImGui.DragInt(label, ref value, speed, minValue, maxValue);
        return value;
    }

    public static int SliderInt(string label, int value, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        ImGui.SliderInt(label, ref value, minValue, maxValue);
        return value;
    }

    public static double DragFloat(string label, double value, double speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        float v = (float)value;
        ImGui.DragFloat(label, ref v, (float)speed, (float)minValue, (float)maxValue);
        return v;
    }

    public static float DragFloat(string label, float value, float speed = 1, float minValue = float.MinValue, float maxValue = float.MaxValue)
    {
        float v = value;
        ImGui.DragFloat(label, ref v, speed, minValue, maxValue);
        return v;
    }

    // maxlength can't just be set to a really large number, it will actually allocate a byte array of that size
    public static string InputText(string label, string value, uint maxLength = 512, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        string v = value;
        ImGui.InputText(label, ref v, maxLength, flags);
        return v;
    }

    public static Color ColorPicker3(string label, Color col)
    {
        Vector3 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255);
        ImGui.ColorEdit3(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        var a = new Color((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), (255));
        return a;
    }

    public static Color ColorPicker4(string label, Color col)
    {
        Vector4 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255, (float)col.A / 255);
        ImGui.ColorEdit4(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        var a = new Color((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), (byte)(imCol.W * 255));
        return a;
    }

    public static string StringEnumCombo(string label, Type type, string currentName, bool includeNone)
    {
        int current = Enum.TryParse(type, currentName, out object? result) ? (int)result : 0;
        string[] allNames = includeNone
            ? Enum.GetNames(type).Prepend("None").ToArray()
            : Enum.GetNames(type);
        ImGui.Combo(label, ref current, allNames, allNames.Length);
        return Enum.GetName(type, current) ?? "";
    }

    public static T EnumCombo<T>(string label, T currentValue) where T : struct, Enum
    {
        string[] names = Enum.GetNames<T>();
        int valueIndex = Array.FindIndex(names, s => s == Enum.GetName(currentValue));
        ImGui.Combo(label, ref valueIndex, names, names.Length);
        return Enum.Parse<T>(names[valueIndex]);
    }

    public static void WithDisabled(bool disabled, Action a)
    {
        if (disabled) ImGui.BeginDisabled();
        a();
        if (disabled) ImGui.EndDisabled();
    }

    public static bool WithDisabled(bool disabled, Func<bool> a)
    {
        if (disabled) ImGui.BeginDisabled();
        bool res = a();
        if (disabled) ImGui.EndDisabled();
        return !disabled && res;
    }

    public static bool WithDisabledButton(bool disabled, string label)
    {
        return WithDisabled(disabled, () => ImGui.Button(label));
    }

    public static bool DragFloatHistory(string label, double value, Action<double> changeCommand, CommandHistory cmd, double speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        double oldVal = value;
        double newVal = DragFloat(label, value, speed, minValue, maxValue);
        if (newVal != (float)oldVal)
        {
            cmd.Add(new PropChangeCommand<double>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }

    public static bool DragIntHistory(string label, int value, Action<int> changeCommand, CommandHistory cmd, int speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        int oldVal = value;
        int newVal = DragInt(label, value, speed, minValue, maxValue);
        if (newVal != oldVal)
        {
            cmd.Add(new PropChangeCommand<int>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }
}