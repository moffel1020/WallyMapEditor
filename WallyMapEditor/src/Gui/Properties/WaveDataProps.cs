using System;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowWaveDataProps(WaveData w, CommandHistory cmd, PropertiesWindowData data)
    {
        if (data.Level is not null)
            RemoveButton(w, data.Level.Desc, cmd);
        ImGui.Separator();

        bool propChanged = false;
        ImGui.Text("ID: " + w.ID);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed", w.Speed, 8, val => w.Speed = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed3", w.Speed3, w.Speed ?? 8, val => w.Speed3 = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.DragNullableDoubleHistory("Speed4", w.Speed4, w.Speed3 ?? w.Speed ?? 8, val => w.Speed4 = val, cmd, minValue: 0);
        // use nullable-like editing to be more user friendly (0 means no loop)
        propChanged |= ImGuiExt.DragNullableUIntHistory("LoopIdx", w.LoopIdx == 0 ? null : w.LoopIdx, 1, val => w.LoopIdx = val ?? 0, cmd, minValue: 1, maxValue: (uint)w.Groups.Length - 1);
        if (ImGui.CollapsingHeader($"CustomPaths##props{w.GetHashCode()}"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("##custompathslist", w.CustomPaths, val => w.CustomPaths = val,
            () => CreateNewCustomPath(w),
            index =>
            {
                bool changed = false;
                CustomPath cp = w.CustomPaths[index];
                if (ImGui.TreeNode($"CustomPath {MapOverviewWindow.GetExtraObjectInfo(cp)}###customPaths{cp.GetHashCode()}"))
                {
                    changed |= ShowProperties(cp, cmd, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##select{cp.GetHashCode()}")) data.Selection.Object = cp;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }
        if (ImGui.CollapsingHeader($"Groups##props{w.GetHashCode()}"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("##groupslist", w.Groups, val => w.Groups = val,
            () => CreateNewGroup(w),
            index =>
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
        propChanged |= ImGuiExt.EditArrayHistory($"##custompathPoints{cp.GetHashCode()}", cp.Points, val => cp.Points = val,
        () => CreateNewPoint(cp),
        index =>
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
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_COUNT_TOOLTIP);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Count", g.Count, 1, val => g.Count = val, cmd);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Count3", g.Count3, g.Count ?? 1, val => g.Count3 = val, cmd);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Count4", g.Count4, g.Count3 ?? g.Count ?? 1, val => g.Count4 = val, cmd);
        ImGui.SeparatorText("Delay");
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_DELAY_TOOLTIP);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Delay", g.Delay, 0, val => g.Delay = val, cmd, speed: 100);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Delay3", g.Delay3, g.Delay ?? 0, val => g.Delay3 = val, cmd, speed: 100);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Delay4", g.Delay4, g.Delay3 ?? g.Delay ?? 0, val => g.Delay4 = val, cmd, speed: 100);
        ImGui.SeparatorText("Stagger");
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_STAGGER_TOOLTIP);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Stagger", g.Stagger, 500, val => g.Stagger = val, cmd, speed: 100);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Stagger3", g.Stagger3, g.Stagger ?? 500, val => g.Stagger3 = val, cmd, speed: 100);
        propChanged |= ImGuiExt.DragNullableUIntHistory("Stagger4", g.Stagger4, g.Stagger3 ?? g.Stagger ?? 500, val => g.Stagger4 = val, cmd, speed: 100);
        ImGui.SeparatorText("Demon movement");
        propChanged |= ImGuiExt.EnumComboHistory("Dir", g.Dir, val => g.Dir = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_DIR_TOOLTIP);
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
            ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_PATH_NUMERIC_TOOLTIP);
        }
        else
        {
            if (MapUtils.IsSharedPath(g.Path))
            {
                cmd.Add(new PropChangeCommand<PathEnum>(val => g.Path = val, g.Path, PathEnum.ANY));
                propChanged = true;
            }
            propChanged |= ImGuiExt.EnumComboHistory("Path##enum", g.Path, val => g.Path = val, cmd);
            ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_PATH_TOOLTIP);
        }
        propChanged |= ImGuiExt.GenericStringComboHistory("Behavior", g.Behavior, val => g.Behavior = val, BehaviorToString, ParseBehaviorString, Enum.GetValues<BehaviorEnum>(), cmd);
        bool realIsShared = MapUtils.IsSharedDir(g.Dir) || g.Shared;
        using (ImGuiExt.DisabledIf(MapUtils.IsSharedDir(g.Dir)))
            propChanged |= ImGuiExt.CheckboxHistory("Shared", realIsShared, val => g.Shared = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_SHARED_TOOLTIP);
        bool realIsSharedPath = MapUtils.IsSharedPath(g.Path) || g.SharedPath;
        using (ImGuiExt.DisabledIf(MapUtils.IsSharedPath(g.Path)))
            propChanged |= ImGuiExt.CheckboxHistory("SharedPath", realIsSharedPath, val => g.SharedPath = val, cmd);
        ImGuiExt.HintTooltip(Strings.UI_HORDE_GROUP_SHARED_PATH_TOOLTIP);
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

    public static WaveData DefaultWaveData(LevelDesc desc) => new()
    {
        // max ID + 1, or ID 0 if none exist
        ID = desc.WaveDatas.Select(w => w.ID + 1).DefaultIfEmpty(0u).Max(),
        LoopIdx = 0,
        CustomPaths = [],
        Groups = [],
    };

    private static Maybe<Point> CreateNewPoint(CustomPath cp)
    {
        Maybe<Point> result = new();
        if (ImGui.Button("Add new point##custompath"))
        {
            result = DefaultPoint;
        }
        return result.DoIfSome(p => p.Parent = cp);
    }
    public static Point DefaultPoint => new() { X = 0, Y = 0 };

    private static Maybe<CustomPath> CreateNewCustomPath(WaveData w)
    {
        Maybe<CustomPath> result = new();
        if (ImGui.Button("Add new custom path##wave"))
        {
            result = DefaultCustomPath;
        }
        return result.DoIfSome(cp => cp.Parent = w);
    }
    public static CustomPath DefaultCustomPath => new() { Points = [] };

    private static Maybe<Group> CreateNewGroup(WaveData w)
    {
        Maybe<Group> result = new();
        if (ImGui.Button("Add new group##wave"))
        {
            result = DefaultGroup;
        }
        return result.DoIfSome(g => g.Parent = w);
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