using System;
using System.Linq;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowWaveDataProps(WaveData w, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("ID: " + w.ID);
        propChanged |= ImGuiExt.DragNullableFloatHistory("Speed", w.Speed, 8, val => w.Speed = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableFloatHistory("Speed3", w.Speed3, w.Speed ?? 8, val => w.Speed3 = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableFloatHistory("Speed4", w.Speed4, w.Speed3 ?? w.Speed ?? 8, val => w.Speed4 = val, cmd, minValue: 0);
        // use nullable-like editing to be more user friendly (0 means no loop)
        propChanged |= ImGuiExt.DragNullableIntHistory("LoopIdx", w.LoopIdx == 0 ? null : w.LoopIdx, 1, val => w.LoopIdx = val ?? 0, cmd, minValue: 1, maxValue: w.Groups.Length - 1);
        if (ImGui.CollapsingHeader($"CustomPaths##props{w.GetHashCode()}"))
        {
            foreach (CustomPath cp in w.CustomPaths)
            {
                if (ImGui.TreeNode($"CustomPath {MapOverviewWindow.GetExtraObjectInfo(cp)}###customPaths{cp.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(cp, cmd);
                    ImGui.TreePop();
                }
            }
        }
        if (ImGui.CollapsingHeader($"Groups##props{w.GetHashCode()}"))
        {
            foreach (Group g in w.Groups)
            {
                if (ImGui.TreeNode($"Group {MapOverviewWindow.GetExtraObjectInfo(g)}###groups{g.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(g, cmd);
                    ImGui.TreePop();
                }
            }
        }
        return propChanged;
    }

    public static bool ShowCustomPathProps(CustomPath cp, CommandHistory cmd)
    {
        bool propChanged = false;
        if (ImGui.CollapsingHeader($"Points##props{cp.GetHashCode()}"))
        {
            foreach (Point p in cp.Points)
            {
                if (ImGui.TreeNode($"Point {MapOverviewWindow.GetExtraObjectInfo(p)}###points{p.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(p, cmd);
                    ImGui.TreePop();
                }
            }
        }
        return propChanged;
    }

    public static bool ShowPointProps(Point p, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragFloatHistory($"X##props{p.GetHashCode()}", p.X, val => p.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory($"Y##props{p.GetHashCode()}", p.Y, val => p.Y = val, cmd);
        return propChanged;
    }

    public static bool ShowGroupProps(Group g, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.SeparatorText("Count");
        propChanged |= ImGuiExt.DragNullableIntHistory("Count", g.Count, 1, val => g.Count = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Count3", g.Count3, g.Count ?? 1, val => g.Count3 = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Count4", g.Count4, g.Count3 ?? g.Count ?? 1, val => g.Count4 = val, cmd, minValue: 0);
        ImGui.SeparatorText("Delay");
        propChanged |= ImGuiExt.DragNullableIntHistory("Delay", g.Delay, 0, val => g.Delay = val, cmd, speed: 100, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Delay3", g.Delay3, g.Delay ?? 0, val => g.Delay3 = val, cmd, speed: 100, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Delay4", g.Delay4, g.Delay3 ?? g.Delay ?? 0, val => g.Delay4 = val, cmd, speed: 100, minValue: 0);
        ImGui.SeparatorText("Stagger");
        propChanged |= ImGuiExt.DragNullableIntHistory("Stagger", g.Stagger, 500, val => g.Stagger = val, cmd, speed: 100, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Stagger3", g.Stagger3, g.Stagger ?? 500, val => g.Stagger3 = val, cmd, speed: 100, minValue: 0);
        propChanged |= ImGuiExt.DragNullableIntHistory("Stagger4", g.Stagger4, g.Stagger3 ?? g.Stagger ?? 500, val => g.Stagger4 = val, cmd, speed: 100, minValue: 0);
        ImGui.SeparatorText("Demons");
        propChanged |= ImGuiExt.EnumComboHistory("Dir", g.Dir, val => g.Dir = val, cmd);
        // Path is either an enum or an int between 0 and 19
        bool isNumericPath = ImGuiExt.Checkbox("Numeric path", MapUtils.IsSharedPath(g.Path));
        if (isNumericPath)
        {
            if (isNumericPath && !MapUtils.IsSharedPath(g.Path))
            {
                cmd.Add(new PropChangeCommand<PathEnum>(val => g.Path = val, g.Path, 0));
                propChanged = true;
            }
            propChanged |= ImGuiExt.DragIntHistory("Path##numeric", (int)g.Path, val => g.Path = (PathEnum)val, cmd, minValue: 0, maxValue: 19);
        }
        else
        {
            if (!isNumericPath && MapUtils.IsSharedPath(g.Path))
            {
                cmd.Add(new PropChangeCommand<PathEnum>(val => g.Path = val, g.Path, PathEnum.ANY));
                propChanged = true;
            }
            propChanged |= ImGuiExt.EnumComboHistory("Path##enum", g.Path, val => g.Path = val, cmd);
        }



        string behaviorString = GetBehaviorString(g.Behavior);
        string newBehaviorString = ImGuiExt.StringCombo("Behavior", behaviorString, [.. Enum.GetValues<BehaviorEnum>().Select(GetBehaviorString)]);
        BehaviorEnum newBehavior = ParseBehaviorString(newBehaviorString);
        if (g.Behavior != newBehavior)
        {
            cmd.Add(new PropChangeCommand<BehaviorEnum>(val => g.Behavior = val, g.Behavior, newBehavior));
            propChanged = true;
        }

        bool realIsShared = MapUtils.IsSharedDir(g.Dir) || g.Shared;
        ImGuiExt.WithDisabled(MapUtils.IsSharedDir(g.Dir), () =>
        {
            propChanged |= ImGuiExt.CheckboxHistory("Shared", realIsShared, val => g.Shared = val, cmd);
        });

        bool realIsSharedPath = MapUtils.IsSharedPath(g.Path) || g.SharedPath;
        ImGuiExt.WithDisabled(MapUtils.IsSharedPath(g.Path), () =>
        {
            propChanged |= ImGuiExt.CheckboxHistory("SharedPath", realIsSharedPath, val => g.SharedPath = val, cmd);
        });

        return propChanged;
    }

    public static string GetBehaviorString(BehaviorEnum behavior) => behavior switch
    {
        BehaviorEnum.FAST => "yellow",
        BehaviorEnum.TANKY => "red",
        BehaviorEnum.ANY => "random",
        _ => "blue",
    };

    public static BehaviorEnum ParseBehaviorString(string behavior) => behavior switch
    {
        "yellow" => BehaviorEnum.FAST,
        "red" => BehaviorEnum.TANKY,
        "random" => BehaviorEnum.ANY,
        _ => BehaviorEnum._,
    };
}