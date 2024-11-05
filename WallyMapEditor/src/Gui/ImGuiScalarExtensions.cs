using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;

namespace WallyMapEditor;

/*
Order:

bool -> int -> uint -> float -> double -> color3 -> color4 -> enum -> text
then
drag -> slider -> input
then
impl -> functional -> history -> nullable history
*/
public static partial class ImGuiExt
{
    #region Bool
    #region Checkbox

    public static bool Checkbox(string label, bool value)
    {
        ImGui.Checkbox(label, ref value);
        return value;
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

    #endregion
    #endregion

    #region Int
    #region DragInt

    public static int DragInt(string label, int value, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        ImGui.DragInt(label, ref value, speed, minValue, maxValue);
        return value;
    }

    public static bool DragIntHistory(string label, int value, Action<int> changeCommand, CommandHistory cmd, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
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

    public static bool DragNullableIntHistory(string label, int? value, int defaultValue, Action<int?> changeCommand, CommandHistory cmd, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
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

    #endregion
    #region SliderInt

    public static int SliderInt(string label, int value, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        ImGui.SliderInt(label, ref value, minValue, maxValue);
        return value;
    }

    #endregion
    #endregion
    #region UInt
    #region DragUInt

    public static void DragUInt(string label, ref uint value, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
    {
        unsafe
        {
            IntPtr valuePtr = (IntPtr)Unsafe.AsPointer(ref value);
            IntPtr minValuePtr = (IntPtr)(&minValue);
            IntPtr maxValuePtr = (IntPtr)(&maxValue);
            ImGui.DragScalar(label, ImGuiDataType.U32, valuePtr, speed, minValuePtr, maxValuePtr);
        }
    }

    public static uint DragUInt(string label, uint value, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
    {
        DragUInt(label, ref value, speed, minValue, maxValue);
        return value;
    }

    public static bool DragUIntHistory(string label, uint value, Action<uint> changeCommand, CommandHistory cmd, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
    {
        uint oldVal = value;
        uint newVal = DragUInt(label, value, speed, minValue, maxValue);
        if (newVal != oldVal)
        {
            cmd.Add(new PropChangeCommand<uint>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }

    public static bool DragNullableUIntHistory(string label, uint? value, uint defaultValue, Action<uint?> changeCommand, CommandHistory cmd, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
    {
        if (value is not null)
        {
            bool dragged = DragUIntHistory(label, value.Value, val => changeCommand(val), cmd, speed, minValue, maxValue);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<uint?>(changeCommand, value, null));
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
                cmd.Add(new PropChangeCommand<uint?>(changeCommand, value, defaultValue));
                return true;
            }
        }
        return false;
    }

    #endregion
    #region  InputUInt

    public static void InputUInt(string label, ref uint value, uint step = 1, uint stepFast = 100)
    {
        unsafe
        {
            IntPtr valuePtr = (IntPtr)Unsafe.AsPointer(ref value);
            IntPtr stepPtr = (IntPtr)(&step);
            IntPtr stepFastPtr = (IntPtr)(&stepFast);
            ImGui.InputScalar(label, ImGuiDataType.U32, valuePtr, stepPtr, stepFastPtr);
        }
    }

    public static uint InputUInt(string label, uint value, uint step = 1, uint stepFast = 100)
    {
        uint v = value;
        InputUInt(label, ref v, step, stepFast);
        return v;
    }

    public static bool InputUIntHistory(string label, uint value, Action<uint> changeCommand, CommandHistory cmd, uint step = 1, uint stepFast = 100)
    {
        uint oldVal = value;
        uint newVal = InputUInt(label, value, step, stepFast);
        if (newVal != oldVal)
        {
            cmd.Add(new PropChangeCommand<uint>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }

    #endregion
    #endregion
    #region DragDouble

    public static void DragDouble(string label, ref double value, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        unsafe
        {
            IntPtr valuePtr = (IntPtr)Unsafe.AsPointer(ref value);
            IntPtr minValuePtr = (IntPtr)(&minValue);
            IntPtr maxValuePtr = (IntPtr)(&maxValue);
            ImGui.DragScalar(label, ImGuiDataType.Double, valuePtr, speed, minValuePtr, maxValuePtr);
        }
    }

    public static double DragDouble(string label, double value, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        double v = value;
        DragDouble(label, ref v, speed, minValue, maxValue);
        return v;
    }

    public static bool DragDoubleHistory(string label, double value, Action<double> changeCommand, CommandHistory cmd, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        double oldVal = value;
        double newVal = DragDouble(label, value, speed, minValue, maxValue);
        // prevent NaN from fucking up history
        if (newVal != oldVal && (!double.IsNaN(newVal) || !double.IsNaN(oldVal)))
        {
            cmd.Add(new PropChangeCommand<double>(changeCommand, oldVal, newVal));
            return true;
        }

        return false;
    }

    public static bool DragNullableDoubleHistory(string label, double? value, double defaultValue, Action<double?> changeCommand, CommandHistory cmd, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
    {
        if (value is not null)
        {
            bool dragged = DragDoubleHistory(label, value.Value, x => changeCommand(x), cmd, speed, minValue, maxValue);
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

    public static bool DragNullableDoublePairHistory(
        string mainLabel,
        string label1, string label2,
        double? value1, double? value2,
        double default1, double default2,
        Action<double?, double?> changeCommand,
        CommandHistory cmd,
        float speed1 = 1, float speed2 = 1,
        double minValue1 = double.MinValue, double minValue2 = double.MaxValue,
        double maxValue1 = double.MinValue, double maxValue2 = double.MaxValue
    )
    {
        bool propChanged = false;
        if (value1 is not null && value2 is not null)
        {
            propChanged |= DragDoubleHistory(label1, value1.Value, val => changeCommand(val, value2.Value), cmd, speed: speed1, minValue: minValue1, maxValue: maxValue1);
            propChanged |= DragDoubleHistory(label2, value2.Value, val => changeCommand(value1.Value, val), cmd, speed: speed2, minValue: minValue2, maxValue: maxValue2);
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

    #endregion
    #region ColorPicker3

    public static WmsColor ColorPicker3(string label, WmsColor col)
    {
        Vector3 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255);
        ImGui.ColorEdit3(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        WmsColor a = new((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), 255);
        return a;
    }

    public static uint ColorPicker3Hex(string label, uint col)
    {
        byte r = (byte)(col >> 16), g = (byte)(col >> 8), b = (byte)col;
        Vector3 imCol = new((float)r / 255, (float)g / 255, (float)b / 255);
        ImGui.ColorEdit3(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        r = (byte)(imCol.X * 255); g = (byte)(imCol.Y * 255); b = (byte)(imCol.Z * 255);
        return ((uint)r << 16) | ((uint)g << 8) | b;
    }

    public static bool ColorPicker3History(string label, WmsColor value, Action<WmsColor> changeCommand, CommandHistory cmd)
    {
        WmsColor newValue = ColorPicker3(label, value);
        if (value != newValue)
        {
            cmd.Add(new PropChangeCommand<WmsColor>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    public static bool ColorPicker3HexHistory(string label, uint value, Action<uint> changeCommand, CommandHistory cmd)
    {
        uint newValue = ColorPicker3Hex(label, value);
        if (value != newValue)
        {
            cmd.Add(new PropChangeCommand<uint>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    public static bool NullableColorPicker3History(string label, WmsColor? value, WmsColor defaultValue, Action<WmsColor?> changeCommand, CommandHistory cmd)
    {
        if (value is not null)
        {
            bool changed = ColorPicker3History(label, value.Value, val => changeCommand(val), cmd);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<WmsColor?>(changeCommand, value, null));
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
                cmd.Add(new PropChangeCommand<WmsColor?>(changeCommand, value, defaultValue));
                return true;
            }
        }
        return false;
    }

    #endregion
    #region ColorPicker4

    public static WmsColor ColorPicker4(string label, WmsColor col)
    {
        Vector4 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255, (float)col.A / 255);
        ImGui.ColorEdit4(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        WmsColor a = new((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), (byte)(imCol.W * 255));
        return a;
    }

    #endregion
    #region Enum

    public static string StringCombo(string label, string value, string[] options)
    {
        int valueIndex = Array.FindIndex(options, s => s == value);
        if (valueIndex == -1) valueIndex = 0; // prevent out of bounds if value is not in options
        ImGui.Combo(label, ref valueIndex, options, options.Length);
        return options[valueIndex];
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

    public static bool GenericStringComboHistory<T>(string label, T value, Action<T> changeCommand, Func<T, string> stringify, Func<string, T> parse, T[] options, CommandHistory cmd)
    {
        string valueString = stringify(value);
        string newValueString = StringCombo(label, valueString, [.. options.Select(stringify)]);
        T newValue = parse(newValueString);
        if (!Equals(value, newValue))
        {
            cmd.Add(new PropChangeCommand<T>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    #endregion
    #region Text
    #region InputText

    // maxlength can't just be set to a really large number, it will actually allocate a byte array of that size
    public static string InputText(string label, string value, uint maxLength = 512, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        string v = value;
        ImGui.InputText(label, ref v, maxLength, flags);
        return v;
    }

    public static string InputTextWithCallback(string label, string value, ImGuiInputTextCallback callback, uint maxLength = 512, ImGuiInputTextFlags flags = ImGuiInputTextFlags.CallbackCharFilter)
    {
        string v = value;
        ImGui.InputText(label, ref v, maxLength, flags, callback);
        return v;
    }

    public static string InputTextMultiline(string label, string value, Vector2 size, uint maxLength = 1024, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        string v = value;
        ImGui.InputTextMultiline(label, ref v, maxLength, size, flags);
        return v;
    }

    public static bool InputTextHistory(string label, string value, Action<string> changeCommand, CommandHistory cmd, uint maxLength = 512, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        string newValue = InputText(label, value, maxLength, flags);
        if (value != newValue)
        {
            cmd.Add(new PropChangeCommand<string>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    #endregion
    #region InputTextWithFilter

    // type ImGuiInputTextCallback
    private unsafe static int NumAlphaFilter(ImGuiInputTextCallbackData* data) => (char)data->EventChar switch
    {
        >= 'a' and <= 'z' => 0,
        >= 'A' and <= 'Z' => 0,
        >= '0' and <= '9' => 0,
        _ => 1,
    };

    public static unsafe string InputTextWithFilter(string label, string value, uint maxLength = 512) => InputTextWithCallback(label, value, NumAlphaFilter, maxLength: maxLength, flags: ImGuiInputTextFlags.CallbackCharFilter);

    public static bool InputTextWithFilterHistory(string label, string value, Action<string> changeCommand, CommandHistory cmd, uint maxLength = 512)
    {
        string newValue = InputTextWithFilter(label, value, maxLength);
        if (value != newValue)
        {
            cmd.Add(new PropChangeCommand<string>(changeCommand, value, newValue));
            return true;
        }
        return false;
    }

    public static bool InputNullableTextWithFilterHistory(string label, string? value, string defaultValue, Action<string?> changeCommand, CommandHistory cmd, uint maxLength = 512)
    {
        if (value is not null)
        {
            bool dragged = InputTextWithFilterHistory(label, value, x => changeCommand(x), cmd, maxLength);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
            {
                cmd.Add(new PropChangeCommand<string?>(changeCommand, value, null));
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
                cmd.Add(new PropChangeCommand<string?>(changeCommand, value, defaultValue));
                return true;
            }
        }
        return false;
    }

    #endregion
    #endregion
}