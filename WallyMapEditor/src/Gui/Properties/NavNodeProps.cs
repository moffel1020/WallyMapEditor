using System;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;
using System.Collections.Generic;

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

        if (data.Level is not null)
        {
            string newIDText = ImGuiExt.InputText("Change NavID", n.NavID.ToString(), flags: ImGuiInputTextFlags.CharsDecimal);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("If the new ID already exists this NavID will not be renamed");

            if (int.TryParse(newIDText, out int newID))
            {
                if (!NavIDExists(newID, data.Level))
                {
                    cmd.Add(new PropChangeCommand<int>(val => RenameNavID(n, val, data.Level), n.NavID, newID));
                    cmd.SetAllowMerge(false);
                }
            }
        }

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

    private static bool RenameNavID(NavNode toRename, int newID, Level l)
    {
        if (NavIDExists(newID, l)) return false;

        int oldID = toRename.NavID;
        toRename.NavID = newID;

        foreach (NavNode node in GetAllNavNodes(l))
        {
            for (int i = 0; i < node.Path.Length; i++)
            {
                (int id, NavNodeTypeEnum type) = node.Path[i];
                if (id == oldID)
                    node.Path[i] = (newID, type);
            }
        }

        return true;
    }

    private static IEnumerable<NavNode> GetAllNavNodes(Level l)
    {
        foreach (NavNode node in l.Desc.NavNodes)
        {
            yield return node;
        }

        foreach (DynamicNavNode dynamic in l.Desc.DynamicNavNodes)
        {
            foreach (NavNode node in dynamic.Children)
            {
                yield return node;
            }
        }
    }

    private static bool NavIDExists(int id, Level l) =>
        GetAllNavNodes(l).Any(n => n.NavID == id);
}