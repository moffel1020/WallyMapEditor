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
        var a = new Color((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), 255);
        return a;
    }

    public static Color ColorPicker4(string label, Color col)
    {
        Vector4 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255, (float)col.A / 255);
        ImGui.ColorEdit4(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        var a = new Color((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), (byte)(imCol.W * 255));
        return a;
    }

    public static string StringCombo(string label, string value, string[] options)
    {
        int valueIndex = Array.FindIndex(options, s => s == value);
        if (valueIndex == -1) valueIndex = 0; // prevent out of bounds if value is not in options
        ImGui.Combo(label, ref valueIndex, options, options.Length);
        return options[valueIndex];
    }

    public static string StringEnumCombo(string label, Type type, string currentName, bool includeNone)
    {
        int current = Enum.TryParse(type, currentName, out object? result) ? (int)result : 0;
        string[] allNames = includeNone
            ? ["None", .. Enum.GetNames(type)]
            : Enum.GetNames(type);
        ImGui.Combo(label, ref current, allNames, allNames.Length);
        return Enum.GetName(type, current) ?? "";
    }

    public static E EnumCombo<E>(string label, E currentValue) where E : struct, Enum
    {
        return Enum.Parse<E>(StringCombo(label, Enum.GetName(currentValue)!, Enum.GetNames<E>()));
    }

    public static E? EnumComboWithNone<E>(string label, E? currentValue) where E : struct, Enum
    {
        return
        Enum.TryParse(StringCombo(label, currentValue is null ? "None" : Enum.GetName(currentValue.Value)!, ["None", .. Enum.GetNames<E>()]), out E e)
            ? e
            : null;
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

    public static bool DragNullableFloatHistory(string label, double? value, double defaultValue, Action<double?> changeCommand, CommandHistory cmd, double speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        if (value is not null)
        {
            bool dragged = DragFloatHistory(label, value.Value, x => changeCommand(x), cmd, speed, minValue, maxValue);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<double?>(changeCommand, value, null));
                return true;
            }
            return dragged;
        }
        else
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + label))
            {
                cmd.Add(new PropChangeCommand<double?>(changeCommand, value, defaultValue));
                return true;
            }
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

    public static bool DragNullableIntHistory(string label, int? value, int defaultValue, Action<int?> changeCommand, CommandHistory cmd, int speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        if (value is not null)
        {
            bool dragged = DragIntHistory(label, value.Value, val => changeCommand(val), cmd, speed, minValue, maxValue);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<int?>(changeCommand, value, null));
                return true;
            }
            return dragged;
        }
        else
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + label))
            {
                cmd.Add(new PropChangeCommand<int?>(changeCommand, value, defaultValue));
                return true;
            }
        }
        return false;
    }

    public static bool CheckboxHistory(string label, bool value, Action<bool> changeCommand, CommandHistory cmd)
    {
        bool oldVal = value;
        bool newVal = Checkbox(label, value);
        if (newVal != oldVal)
        {
            cmd.Add(new PropChangeCommand<bool>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }

    public static bool NullableCheckboxHistory(string label, bool? value, bool defaultValue, Action<bool?> changeCommand, CommandHistory cmd)
    {
        if (value is not null)
        {
            bool changed = CheckboxHistory(label, value.Value, val => changeCommand(val), cmd);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<bool?>(changeCommand, value, null));
                return true;
            }
            return changed;
        }
        else
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + label))
            {
                cmd.Add(new PropChangeCommand<bool?>(changeCommand, value, defaultValue));
                return true;
            }
        }
        return false;
    }

    public static bool DragNullableFloatPairHistory(
        string mainLabel,
        string label1, string label2,
        double? value1, double? value2,
        double default1, double default2,
        Action<double?, double?> changeCommand,
        CommandHistory cmd,
        double speed1 = 1, double speed2 = 1,
        double minValue1 = double.MinValue, double minValue2 = double.MaxValue,
        double maxValue1 = double.MinValue, double maxValue2 = double.MaxValue
    )
    {
        bool propChanged = false;
        if (value1 is not null && value2 is not null)
        {
            propChanged |= DragFloatHistory(label1, value1.Value, val => changeCommand(val, value2.Value), cmd, speed: speed1, minValue: minValue1, maxValue: maxValue1);
            propChanged |= DragFloatHistory(label2, value2.Value, val => changeCommand(value1.Value, val), cmd, speed: speed2, minValue: minValue2, maxValue: maxValue2);
            if (ImGui.Button("Remove##" + mainLabel))
            {
                cmd.Add(new PropChangeCommand<(double?, double?)>(
                    val => changeCommand(val.Item1, val.Item2),
                    (value1, value2),
                    (null, null)
                ));
                return true;
            }
        }
        else
        {
            ImGui.Text(mainLabel);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + mainLabel))
            {
                cmd.Add(new PropChangeCommand<(double?, double?)>(
                    val => changeCommand(val.Item1, val.Item2),
                    (value1, value2),
                    (default1, default2)
                ));
                return true;
            }
        }
        return propChanged;
    }

    public static bool EnumComboHistory<T>(string label, T value, Action<T> changeCommand, CommandHistory cmd) where T : struct, Enum
    {
        T newValue = EnumCombo(label, value);
        if (!value.Equals(newValue))
        {
            cmd.Add(new PropChangeCommand<T>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    public static bool NullableEnumComboHistory<T>(string label, T? value, Action<T?> changeCommand, CommandHistory cmd) where T : struct, Enum
    {
        T? newValue = EnumComboWithNone(label, value);
        if (!value.Equals(newValue))
        {
            cmd.Add(new PropChangeCommand<T?>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }
}