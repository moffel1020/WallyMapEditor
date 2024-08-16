using System;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowWaveDataProps(WaveData w, CommandHistory cmd, PropertiesWindowData data)
    {
        bool propChanged = false;
        ImGui.Text("ID: " + w.ID);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed", w.Speed, 8, val => w.Speed = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed3", w.Speed3, w.Speed ?? 8, val => w.Speed3 = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed4", w.Speed4, w.Speed3 ?? w.Speed ?? 8, val => w.Speed4 = val, cmd, minValue: 0);
        // use nullable-like editing to be more user friendly (0 means no loop)
        propChanged |= ImGuiExt.DragNullableIntHistory("LoopIdx", w.LoopIdx == 0 ? null : w.LoopIdx, 1, val => w.LoopIdx = val ?? 0, cmd, minValue: 1, maxValue: w.Groups.Length - 1);
        if (ImGui.CollapsingHeader($"CustomPaths##props{w.GetHashCode()}"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("##custompathslist", w.CustomPaths, val => w.CustomPaths = val,
            CreateNewCustomPath,
            (int index) =>
            {
                bool changed = false;
                CustomPath cp = w.CustomPaths[index];
                if (ImGui.TreeNode($"CustomPath {MapOverviewWindow.GetExtraObjectInfo(cp)}###customPaths{cp.GetHashCode()}"))
                {
                    changed |= ShowProperties(cp, cmd, data);
                    ImGui.TreePop();
                }
                return changed;
            }, cmd);
        }
        if (ImGui.CollapsingHeader($"Groups##props{w.GetHashCode()}"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("##groupslist", w.Groups, val => w.Groups = val,
            CreateNewGroup,
            (int index) =>
            {
                bool changed = false;
                Group g = w.Groups[index];
                if (ImGui.TreeNode($"Group {MapOverviewWindow.GetExtraObjectInfo(g)}###groups{g.GetHashCode()}"))
                {
                    changed |= ShowProperties(g, cmd, data);
                    ImGui.TreePop();
                }
                return changed;
            }, cmd);
        }
        return propChanged;
    }

    public static bool ShowCustomPathProps(CustomPath cp, CommandHistory cmd, PropertiesWindowData data)
    {
        bool propChanged = false;
        if (ImGui.CollapsingHeader($"Points##props{cp.GetHashCode()}"))
        {
            propChanged |= ImGuiExt.EditArrayHistory($"##custompathPoints{cp.GetHashCode()}", cp.Points, val => cp.Points = val,
            CreateNewPoint,
            (int index) =>
            {
                bool changed = false;
                Point p = cp.Points[index];
                if (ImGui.TreeNode($"Point {MapOverviewWindow.GetExtraObjectInfo(p)}###points{p.GetHashCode()}"))
                {
                    changed |= ShowProperties(p, cmd, data);
                    ImGui.TreePop();
                }
                return changed;
            }, cmd);
        }
        return propChanged;
    }

    public static bool ShowPointProps(Point p, CommandHistory cmd)
    {
        bool propChanged = false;
        propChanged |= ImGuiExt.DragDoubleHistory($"X##props{p.GetHashCode()}", p.X, val => p.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"Y##props{p.GetHashCode()}", p.Y, val => p.Y = val, cmd);
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
            if (!MapUtils.IsSharedPath(g.Path))
            {
                cmd.Add(new PropChangeCommand<PathEnum>(val => g.Path = val, g.Path, 0));
                propChanged = true;
            }
            propChanged |= ImGuiExt.DragIntHistory("Path##numeric", (int)g.Path, val => g.Path = (PathEnum)val, cmd, minValue: 0, maxValue: 19);
        }
        else
        {
            if (MapUtils.IsSharedPath(g.Path))
            {
                cmd.Add(new PropChangeCommand<PathEnum>(val => g.Path = val, g.Path, PathEnum.ANY));
                propChanged = true;
            }
            propChanged |= ImGuiExt.EnumComboHistory("Path##enum", g.Path, val => g.Path = val, cmd);
        }
        propChanged |= ImGuiExt.GenericStringComboHistory("Behavior", g.Behavior, val => g.Behavior = val, BehaviorToString, ParseBehaviorString, Enum.GetValues<BehaviorEnum>(), cmd);
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

    public static string BehaviorToString(BehaviorEnum behavior) => behavior switch
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

    private static Maybe<Point> CreateNewPoint()
    {
        Maybe<Point> result = new();
        if (ImGui.Button("Add new point##custompath"))
        {
            result = DefaultPoint;
        }
        return result;
    }
    public static Point DefaultPoint => new() { X = 0, Y = 0 };

    private static Maybe<CustomPath> CreateNewCustomPath()
    {
        Maybe<CustomPath> result = new();
        if (ImGui.Button("Add new custom path##wave"))
        {
            result = DefaultCustomPath;
        }
        return result;
    }
    public static CustomPath DefaultCustomPath => new() { Points = [] };

    private static Maybe<Group> CreateNewGroup()
    {
        Maybe<Group> result = new();
        if (ImGui.Button("Add new group##wave"))
        {
            result = DefaultGroup;
        }
        return result;
    }
    public static Group DefaultGroup => new()
    {
        Count = 1,
        Delay = 0,
        Stagger = 500,
        Dir = DirEnum.ANY,
        Path = PathEnum.ANY,
        Behavior = BehaviorEnum._
    };
}