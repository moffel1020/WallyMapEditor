using System;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowNavNodeProps(NavNode n, CommandHistory cmd, PropertiesWindowData data)
    {
        if (n.Parent is not null)
        {
            ImGui.Text("Parent DynamicNavNode: ");
            ImGui.SameLine();
            if (ImGui.Button($"PlatID {n.Parent.PlatID}")) data.Selection.Object = n.Parent;
            ImGui.Separator();
        }
        bool propChanged = false;
        ImGui.Text("NavID: " + n.NavID);
        propChanged |= ImGuiExt.GenericStringComboHistory("NavType", n.Type, val => n.Type = val,
        NavTypeToString, ParseNavTypeString,
        [.. Enum.GetValues<NavNodeTypeEnum>().Where(t => t != NavNodeTypeEnum.D)], cmd);

        ImGui.TextWrapped("Path: " + string.Join(", ", n.Path.Select(nn => nn.Item1)));
        propChanged |= ImGuiExt.DragDoubleHistory("X", n.X, val => n.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", n.Y, val => n.Y = val, cmd);
        return propChanged;
    }

    public static string NavTypeToString(NavNodeTypeEnum t) => t switch
    {
        not NavNodeTypeEnum._ => t.ToString(),
        _ => "None",
    };

    public static NavNodeTypeEnum ParseNavTypeString(string s) => s switch
    {
        "None" => NavNodeTypeEnum._,
        _ => Enum.Parse<NavNodeTypeEnum>(s),
    };
}