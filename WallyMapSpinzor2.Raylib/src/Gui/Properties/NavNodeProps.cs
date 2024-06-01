using System;
using System.Linq;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowNavNodeProps(NavNode n, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("NavID: " + n.NavID);
        string navTypeString = GetNavTypeString(n.Type);
        string newNavTypeString = ImGuiExt.StringCombo("NavType", navTypeString, NAV_TYPE_STRINGS);
        NavNodeTypeEnum newNavType = ParseNavTypeString(newNavTypeString);
        if (n.Type != newNavType)
        {
            cmd.Add(new PropChangeCommand<NavNodeTypeEnum>(val => n.Type = val, n.Type, newNavType));
            propChanged = true;
        }
        ImGui.TextWrapped("Path: " + string.Join(", ", n.Path.Select(nn => nn.Item1)));
        propChanged |= ImGuiExt.DragFloatHistory("X", n.X, val => n.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", n.Y, val => n.Y = val, cmd);
        return propChanged;
    }

    private static readonly string[] NAV_TYPE_STRINGS = [.. Enum.GetValues<NavNodeTypeEnum>().Select(GetNavTypeString).Distinct()];

    public static string GetNavTypeString(NavNodeTypeEnum type) => type switch
    {
        NavNodeTypeEnum.W => "W",
        NavNodeTypeEnum.A => "A",
        NavNodeTypeEnum.L => "L",
        NavNodeTypeEnum.G => "G",
        NavNodeTypeEnum.T => "T",
        NavNodeTypeEnum.S => "S",
        _ => "",
    };

    public static NavNodeTypeEnum ParseNavTypeString(string type) => type switch
    {
        "W" => NavNodeTypeEnum.W,
        "A" => NavNodeTypeEnum.A,
        "L" => NavNodeTypeEnum.L,
        "G" => NavNodeTypeEnum.G,
        "T" => NavNodeTypeEnum.T,
        "S" => NavNodeTypeEnum.S,
        _ => NavNodeTypeEnum._,
    };
}