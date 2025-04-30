using System;
using System.Linq;
using System.Collections.Generic;
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

        if (data.Level is not null)
        {
            bool removed = RemoveButton(n, cmd, GetNavNodeParentArray(n, data.Level.Desc), SetNavNodeParentArray(n, data.Level.Desc));
            ImGui.Separator();

            string newIDText = ImGuiExt.InputText("Change NavID", n.NavID.ToString(), flags: ImGuiInputTextFlags.CharsDecimal);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("If the new ID already exists this NavID will not be renamed");

            if (!removed && uint.TryParse(newIDText, out uint newID))
            {
                if (!NavIDExists(newID, data.Level.Desc))
                {
                    cmd.Add(new PropChangeCommand<uint>(val => RenameNavID(n, val, data.Level.Desc), n.NavID, newID), false);
                }
            }
        }

        propChanged |= ImGuiExt.GenericStringComboHistory("NavType", n.Type, val => n.Type = val,
            NavTypeToString, ParseNavTypeString,
            [.. Enum.GetValues<NavNodeTypeEnum>().Where(t => t != NavNodeTypeEnum.D)], cmd);

        ImGui.Separator();
        ImGui.Text("Path:");

        const int BUTTONS_PER_LINE = 8;
        Action? remove = null;
        for (int i = 0; i < n.Path.Length; i++)
        {
            if (ImGui.Button(n.Path[i].Item1.ToString() + "##navpath"))
            {
                int index = i;
                remove = () =>
                {
                    // no need for ArrayRemoveCommand because we're not dealing with selectables
                    (uint, NavNodeTypeEnum)[] result = WmeUtils.RemoveAt(n.Path, index);
                    cmd.Add(new PropChangeCommand<(uint, NavNodeTypeEnum)[]>(val => n.Path = val, n.Path, result), false);
                    propChanged = true;
                };
            }
            if (i % BUTTONS_PER_LINE != BUTTONS_PER_LINE - 1 && i != n.Path.Length - 1) ImGui.SameLine();
        }
        if (remove is not null) remove();

        if (data.Level is not null)
        {
            ImGui.SetNextItemWidth(95);
            if (ImGui.BeginCombo("##addnavpath", "Add id to path", ImGuiComboFlags.NoArrowButton))
            {
                foreach (NavNode node in EnumerateNavNodes(data.Level.Desc).Where(nav => !n.Path.Select(p => p.Item1).Contains(nav.NavID)).OrderBy(n => n.NavID))
                {
                    if (node.NavID != n.NavID && ImGui.Selectable($"{node.NavID}###pathselect{node.GetHashCode()}"))
                    {
                        // no need for ArrayAddCommand because we're not dealing with selectables
                        cmd.Add(new PropChangeCommand<(uint, NavNodeTypeEnum)[]>(val => n.Path = val, n.Path, [.. n.Path, (node.NavID, node.Type)]), false);
                    }
                }
                ImGui.EndCombo();
            }
        }
        ImGui.Separator();

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

    private static bool RenameNavID(NavNode toRename, uint newID, LevelDesc ld)
    {
        if (NavIDExists(newID, ld)) return false;

        uint oldID = toRename.NavID;
        toRename.NavID = newID;

        foreach (NavNode node in EnumerateNavNodes(ld))
        {
            for (int i = 0; i < node.Path.Length; i++)
            {
                (uint id, NavNodeTypeEnum type) = node.Path[i];
                if (id == oldID)
                    node.Path[i] = (newID, type);
            }
        }

        return true;
    }

    private static IEnumerable<NavNode> EnumerateNavNodes(LevelDesc ld)
    {
        foreach (NavNode node in ld.NavNodes)
        {
            yield return node;
        }

        foreach (DynamicNavNode dynamic in ld.DynamicNavNodes)
        {
            foreach (NavNode node in dynamic.Children)
            {
                yield return node;
            }
        }
    }

    private static bool NavIDExists(uint id, LevelDesc ld) =>
        EnumerateNavNodes(ld).Any(n => n.NavID == id);

    public static NavNode DefaultNavNode(double posX, double posY, LevelDesc desc) => new()
    {
        X = posX,
        Y = posY,
        NavID = EnumerateNavNodes(desc).Select(n => n.NavID).OrderByDescending(id => id).FirstOrDefault() + 1,
        Path = [],
        Type = NavNodeTypeEnum.A
    };

    private static NavNode[] GetNavNodeParentArray(NavNode n, LevelDesc desc) =>
        n.Parent is null ? desc.NavNodes : n.Parent.Children;

    private static Action<NavNode[]> SetNavNodeParentArray(NavNode n, LevelDesc desc) =>
        n.Parent is null
            ? val => desc.NavNodes = val
            : val => n.Parent.Children = val;
}