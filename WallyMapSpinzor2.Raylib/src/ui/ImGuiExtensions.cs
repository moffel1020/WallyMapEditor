using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace WallyMapSpinzor2.Raylib;

// ImGui is a static class and for some reason you can't make extensions on a static class so this is used instead
public static class ImGuiExt
{
    public static bool Checkbox(string label, bool value)
    {
        ImGui.Checkbox(label, ref value);
        return value;
    }

    public static double DragFloat(string label, double value)
    {
        float v = (float)value;
        ImGui.DragFloat(label, ref v);
        return v;
    }

    public static Color ColorPicker4(string label, Color col)
    {
        Vector4 imCol = new((float)col.R / 255, (float)col.G / 255, (float)col.B / 255, (float)col.A / 255);
        ImGui.ColorEdit4(label, ref imCol, ImGuiColorEditFlags.NoInputs);
        var a = new Color((byte)(imCol.X * 255), (byte)(imCol.Y * 255), (byte)(imCol.Z * 255), (byte)(imCol.W * 255));
        return a;
    }

    public static string StringEnumCombo(string label, Type type, string currentName)
    {
        // idk
        int current = Enum.TryParse(type, currentName, out object? result) ? (int)result : 0;
        List<string> allNames = Enum.GetNames(type).ToList();
        allNames.Insert(0, "None");
        ImGui.Combo(label, ref current, allNames.ToArray(), allNames.Count);
        return Enum.GetName(type, current) ?? "";
    }
}