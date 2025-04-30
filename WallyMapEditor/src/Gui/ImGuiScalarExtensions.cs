using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;

namespace WallyMapEditor;

/*
Order:

bool -> int -> uint -> double -> color3 -> color3 hex -> color4 -> enum -> text
then (when applicable)
drag -> slider -> input
then
impl -> functional -> nullable -> history -> nullable history
*/
public static partial class ImGuiExt
{
    #region Generics
    private static bool GenericDragScalarImpl<T>(ImGuiDataType type, string label, ref T value, float speed, T minValue, T maxValue) where T : unmanaged
    {
        unsafe
        {
            IntPtr valuePtr = (IntPtr)Unsafe.AsPointer(ref value);
            IntPtr minValuePtr = (IntPtr)(&minValue);
            IntPtr maxValuePtr = (IntPtr)(&maxValue);
            return ImGui.DragScalar(label, type, valuePtr, speed, minValuePtr, maxValuePtr);
        }
    }

    private delegate bool ScalarDragDelegate<T>(string label, ref T value, float speed, T maxValue, T minValue);
    private static T GenericDragScalar<T>(ScalarDragDelegate<T> func, string label, T value, float speed, T maxValue, T minValue) where T : struct
    {
        func(label, ref value, speed, maxValue, minValue);
        return value;
    }

    private static bool GenericHistory<T>(T value, T newValue, Action<T> changeCommand, CommandHistory cmd)
    {
        if (!Equals(value, newValue))
        {
            // don't merge with nulls
            cmd.Add(new PropChangeCommand<T>(changeCommand, value, newValue), allowMerge: value is not null);
            return true;
        }
        return false;
    }

    private static T? NullableGeneric<T>(string label, T? value, Func<T, T> modifier, T defaultValue) where T : class
    {
        if (value is not null)
        {
            T newValue = modifier(value);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
                return null;
            return newValue;
        }
        else
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + label))
                return defaultValue;
            return null;
        }
    }

    private static T? NullableGeneric<T>(string label, T? value, Func<T, T> modifier, T defaultValue) where T : struct
    {
        if (value is not null)
        {
            T newValue = modifier(value.Value);
            ImGui.SameLine();
            if (ImGui.Button("Remove##" + label))
                return null;
            return newValue;
        }
        else
        {
            ImGui.Text(label);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + label))
                return defaultValue;
            return null;
        }
    }

    private static bool NullableGenericHistory<T>(string label, T? value, Func<T, T> modifier, T defaultValue, Action<T?> changeCommand, CommandHistory cmd) where T : class
        => GenericHistory(value, NullableGeneric(label, value, modifier, defaultValue), changeCommand, cmd);
    private static bool NullableGenericHistory<T>(string label, T? value, Func<T, T> modifier, T defaultValue, Action<T?> changeCommand, CommandHistory cmd) where T : struct
        => GenericHistory(value, NullableGeneric(label, value, modifier, defaultValue), changeCommand, cmd);

    #endregion
    #region Bool
    #region Checkbox

    public static bool Checkbox(string label, bool value)
    {
        ImGui.Checkbox(label, ref value);
        return value;
    }

    public static bool CheckboxHistory(string label, bool value, Action<bool> changeCommand, CommandHistory cmd)
        => GenericHistory(value, Checkbox(label, value), changeCommand, cmd);
    public static bool NullableCheckboxHistory(string label, bool? value, bool defaultValue, Action<bool?> changeCommand, CommandHistory cmd)
        => NullableGenericHistory(label, value, val => Checkbox(label, val), defaultValue, changeCommand, cmd);

    #endregion
    #endregion
    #region Int
    #region DragInt

    public static int DragInt(string label, int value, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
        => GenericDragScalar(ImGui.DragInt, label, value, speed, minValue, maxValue);
    public static bool DragIntHistory(string label, int value, Action<int> changeCommand, CommandHistory cmd, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
        => GenericHistory(value, DragInt(label, value, speed, minValue, maxValue), changeCommand, cmd);
    public static bool DragNullableIntHistory(string label, int? value, int defaultValue, Action<int?> changeCommand, CommandHistory cmd, float speed = 1, int minValue = int.MinValue, int maxValue = int.MaxValue)
        => NullableGenericHistory(label, value, val => DragInt(label, val, speed, minValue, maxValue), defaultValue, changeCommand, cmd);

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

    public static bool DragUInt(string label, ref uint value, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
        => GenericDragScalarImpl(ImGuiDataType.U32, label, ref value, speed, minValue, maxValue);
    public static uint DragUInt(string label, uint value, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
        => GenericDragScalar(DragUInt, label, value, speed, minValue, maxValue);
    public static bool DragUIntHistory(string label, uint value, Action<uint> changeCommand, CommandHistory cmd, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
        => GenericHistory(value, DragUInt(label, value, speed, minValue, maxValue), changeCommand, cmd);
    public static bool DragNullableUIntHistory(string label, uint? value, uint defaultValue, Action<uint?> changeCommand, CommandHistory cmd, float speed = 1, uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
        => NullableGenericHistory(label, value, val => DragUInt(label, val, speed, minValue, maxValue), defaultValue, changeCommand, cmd);

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
        => GenericHistory(value, InputUInt(label, value, step, stepFast), changeCommand, cmd);

    #endregion
    #endregion
    #region Double
    #region DragDouble

    public static bool DragDouble(string label, ref double value, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
        => GenericDragScalarImpl(ImGuiDataType.Double, label, ref value, speed, minValue, maxValue);
    public static double DragDouble(string label, double value, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
        => GenericDragScalar(DragDouble, label, value, speed, minValue, maxValue);
    public static bool DragDoubleHistory(string label, double value, Action<double> changeCommand, CommandHistory cmd, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
        => GenericHistory(value, DragDouble(label, value, speed, minValue, maxValue), changeCommand, cmd);
    public static bool DragNullableDoubleHistory(string label, double? value, double defaultValue, Action<double?> changeCommand, CommandHistory cmd, float speed = 1, double minValue = double.MinValue, double maxValue = double.MaxValue)
        => NullableGenericHistory(label, value, val => DragDouble(label, val, speed, minValue, maxValue), defaultValue, changeCommand, cmd);

    // this is a monster
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
                cmd.Add(new PropChangeCommand<double?, double?>(changeCommand, value1, value2, null, null));
                return true;
            }
        }
        else
        {
            ImGui.Text(mainLabel);
            ImGui.SameLine();
            if (ImGui.Button("Add##" + mainLabel))
            {
                cmd.Add(new PropChangeCommand<double?, double?>(changeCommand, value1, value2, default1, default2));
                return true;
            }
        }
        return propChanged;
    }

    #endregion
    #endregion
    #region ColorPicker3

    public static WmsColor ColorPicker3(string label, WmsColor col)
    {
        Vector3 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255);
        ImGui.ColorEdit3(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        WmsColor a = new((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), 255);
        return a;
    }
    public static bool NullableColorPicker3History(string label, WmsColor? value, WmsColor defaultValue, Action<WmsColor?> changeCommand, CommandHistory cmd)
        => NullableGenericHistory(label, value, val => ColorPicker3(label, val), defaultValue, changeCommand, cmd);

    #endregion
    #region ColorPicker3 Hex

    public static uint ColorPicker3Hex(string label, uint col)
    {
        byte r = (byte)(col >> 16), g = (byte)(col >> 8), b = (byte)col;
        Vector3 imCol = new((float)r / 255, (float)g / 255, (float)b / 255);
        ImGui.ColorEdit3(label, ref imCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);
        r = (byte)(imCol.X * 255); g = (byte)(imCol.Y * 255); b = (byte)(imCol.Z * 255);
        return ((uint)r << 16) | ((uint)g << 8) | b;
    }
    public static bool ColorPicker3HexHistory(string label, uint value, Action<uint> changeCommand, CommandHistory cmd)
        => GenericHistory(value, ColorPicker3Hex(label, value), changeCommand, cmd);

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
        => Enum.Parse<E>(StringCombo(label, Enum.GetName(currentValue)!, Enum.GetNames<E>()));
    public static E? EnumComboWithNone<E>(string label, E? currentValue) where E : struct, Enum
        => Enum.TryParse(StringCombo(label, currentValue is null ? "None" : Enum.GetName(currentValue.Value)!, ["None", .. Enum.GetNames<E>()]), out E e)
            ? e
            : null;
    public static bool EnumComboHistory<E>(string label, E value, Action<E> changeCommand, CommandHistory cmd) where E : struct, Enum
        => GenericHistory(value, EnumCombo(label, value), changeCommand, cmd);
    public static bool NullableEnumComboHistory<E>(string label, E? value, Action<E?> changeCommand, CommandHistory cmd) where E : struct, Enum
        => GenericHistory(value, EnumComboWithNone(label, value), changeCommand, cmd);
    public static bool GenericStringComboHistory<T>(string label, T value, Action<T> changeCommand, Func<T, string> stringify, Func<string, T> parse, T[] options, CommandHistory cmd)
        => GenericHistory(value, parse(StringCombo(label, stringify(value), [.. options.Select(stringify)])), changeCommand, cmd);

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
        => GenericHistory(value, InputText(label, value, maxLength, flags), changeCommand, cmd);

    #endregion
    #region InputTextWithFilter

    // type ImGuiInputTextCallback
    private unsafe static int NumAlphaFilter(ImGuiInputTextCallbackData* data) => data->EventChar switch
    {
        >= 0x30 /* 0 */ and <= 0x39 /* 9 */ => 0,
        >= 0x41 /* A */ and <= 0x5a /* Z */ => 0,
        >= 0x61 /* a */ and <= 0x6a /* z */ => 0,
        _ => 1,
    };

    public static unsafe string InputTextWithFilter(string label, string value, uint maxLength = 512)
        => InputTextWithCallback(label, value, NumAlphaFilter, maxLength: maxLength, flags: ImGuiInputTextFlags.CallbackCharFilter);
    public static bool InputTextWithFilterHistory(string label, string value, Action<string> changeCommand, CommandHistory cmd, uint maxLength = 512)
        => GenericHistory(value, InputTextWithFilter(label, value, maxLength), changeCommand, cmd);
    public static bool InputNullableTextWithFilterHistory(string label, string? value, string defaultValue, Action<string?> changeCommand, CommandHistory cmd, uint maxLength = 512)
        => NullableGenericHistory(label, value, val => InputTextWithFilter(label, val, maxLength), defaultValue, changeCommand, cmd);

    #endregion
    #endregion
}