using System;
using System.Collections.Generic;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowCollisionProps(AbstractCollision ac, CommandHistory cmd, PropertiesWindowData data) => ac switch
    {
        AbstractPressurePlateCollision pc => ShowAbstractPressurePlateCollisionProps(pc, cmd, data),
        LavaCollision lc => ShowLavaCollisionProps(lc, cmd, data),
        _ => ShowAbstractCollisionProps(ac, cmd, data)
    };

    public static bool ShowAbstractCollisionProps(AbstractCollision ac, CommandHistory cmd, PropertiesWindowData data)
    {
        if (ac.Parent is not null)
        {
            ImGui.Text($"Parent DynamicCollision: ");
            ImGui.SameLine();
            if (ImGui.Button($"PlatID {ac.Parent.PlatID}")) data.Selection.Object = ac.Parent;
            ImGui.Separator();
        }

        bool propChanged = false;

        if (data.Level is not null)
            RemoveButton(ac, cmd, GetColParentArray(ac, data.Level.Desc), SetColParentArray(ac, data.Level.Desc));
        ImGui.Separator();

        if (data.Level is not null) propChanged |= ObjectChangeType(ac, cmd, ShowChangeColTypeMenu, () => ac.Parent?.Children ?? data.Level.Desc.Collisions);
        propChanged |= ImGuiExt.DragDoubleHistory($"X1##props{ac.GetHashCode()}", ac.X1, val => ac.X1 = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"Y1##props{ac.GetHashCode()}", ac.Y1, val => ac.Y1 = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"X2##props{ac.GetHashCode()}", ac.X2, val => ac.X2 = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"Y2##props{ac.GetHashCode()}", ac.Y2, val => ac.Y2 = val, cmd);
        propChanged |= ImGuiExt.GenericStringComboHistory($"Team##props{ac.GetHashCode()}", ac.Team, val => ac.Team = val,
        t => t switch
        {
            0 => "None",
            _ => t.ToString(),
        },
        t => t switch
        {
            "None" => 0,
            _ => int.Parse(t),
        }, [0, 1, 2, 3, 4, 5], cmd);
        propChanged |= ImGuiExt.NullableEnumComboHistory($"Flag##{ac.GetHashCode()}", ac.Flag, val => ac.Flag = val, cmd);
        propChanged |= ImGuiExt.NullableEnumComboHistory($"ColorFlag##{ac.GetHashCode()}", ac.ColorFlag, val => ac.ColorFlag = val, cmd);

        string tauntEventString = ac.TauntEvent ?? "";
        string newTauntEventString = ImGuiExt.InputText($"TauntEvent##{ac.GetHashCode()}", tauntEventString);
        if (tauntEventString != newTauntEventString)
        {
            cmd.Add(new PropChangeCommand<string?>(val => ac.TauntEvent = val, ac.TauntEvent, newTauntEventString == "" ? null : newTauntEventString));
            propChanged = true;
        }

        ImGui.SeparatorText($"Anchor##props{ac.GetHashCode()}");
        propChanged |= ImGuiExt.DragNullableDoublePairHistory(
            "anchor",
            $"AnchorX##props{ac.GetHashCode()}", $"AnchorY##props{ac.GetHashCode()}",
            ac.AnchorX, ac.AnchorY,
            (ac.X1 + ac.X2) / 2 + (ac.Parent?.X ?? 0), (ac.Y1 + ac.Y2) / 2 + (ac.Parent?.Y ?? 0),
            (val1, val2) => (ac.AnchorX, ac.AnchorY) = (val1, val2),
            cmd
        );

        ImGui.SeparatorText($"Normal##props{ac.GetHashCode()}");
        propChanged |= ImGuiExt.DragDoubleHistory($"NormalX##props{ac.GetHashCode()}", ac.NormalX, val => ac.NormalX = val, cmd, speed: 0.01f, minValue: -1, maxValue: 1);
        propChanged |= ImGuiExt.DragDoubleHistory($"NormalY##props{ac.GetHashCode()}", ac.NormalY, val => ac.NormalY = val, cmd, speed: 0.01f, minValue: -1, maxValue: 1);

        return propChanged;
    }

    private const string EMPTY_TRAP_POWERS_WARNING = "Warning: An empty TrapPowers list will crash the game";

    public static bool ShowAbstractPressurePlateCollisionProps(AbstractPressurePlateCollision pc, CommandHistory cmd, PropertiesWindowData data)
    {
        bool propChanged = false;
        propChanged |= ShowAbstractCollisionProps(pc, cmd, data);

        ImGui.SeparatorText($"Pressure plate props##props{pc.GetHashCode()}");
        propChanged |= ShowNullablePlatIDEdit(val => pc.PlatID = val, pc.PlatID, data, cmd);
        if (pc.Parent?.PlatID != pc.PlatID)
            ImGui.TextWrapped("Warning: Pressure plate asset PlatID and parent PlatID do not match");
        ImGui.Separator();

        ImGui.Text("AssetName: " + pc.AssetName);

        if (data.Canvas is not null)
        {
            ImGuiExt.Animation(data.Canvas, pc.Gfx, "Ready", 0);
        }
        propChanged |= ImGuiExt.DragDoubleHistory($"AnimOffseyX##props{pc.GetHashCode()}", pc.AnimOffsetX, val => pc.AnimOffsetX = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"AnimOffsetY##props{pc.GetHashCode()}", pc.AnimOffsetY, val => pc.AnimOffsetY = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"AnimRotation##props{pc.GetHashCode()}", pc.AnimRotation, val => pc.AnimRotation = BrawlhallaMath.SafeMod(val, 360.0), cmd);
        propChanged |= ImGuiExt.DragIntHistory($"Cooldown##props{pc.GetHashCode()}", pc.Cooldown, val => pc.Cooldown = val, cmd, minValue: 0);
        propChanged |= ImGuiExt.CheckboxHistory($"FaceLeft##props{pc.GetHashCode()}", pc.FaceLeft, val => pc.FaceLeft = val, cmd);

        ImGui.SeparatorText("Powers");

        pc.FireOffsetX = NormalizeFireOffset(pc.TrapPowers.Length, pc.FireOffsetX, 0);
        propChanged |= FireOffsetModeEdit("FireOffsetX", pc.TrapPowers.Length, pc.FireOffsetX, val => pc.FireOffsetX = val, cmd);
        bool fireOffsetXShared = pc.FireOffsetX.Length == 1;
        if (fireOffsetXShared)
            propChanged |= ImGuiExt.DragDoubleHistory("shared FireOffsetX", pc.FireOffsetX[0], val => pc.FireOffsetX[0] = val, cmd);

        ImGui.Separator();

        pc.FireOffsetY = NormalizeFireOffset(pc.TrapPowers.Length, pc.FireOffsetY, -10);
        propChanged |= FireOffsetModeEdit("FireOffsetY", pc.TrapPowers.Length, pc.FireOffsetY, val => pc.FireOffsetY = val, cmd);
        bool fireOffsetYShared = pc.FireOffsetY.Length == 1;
        if (fireOffsetYShared)
            propChanged |= ImGuiExt.DragDoubleHistory("shared FireOffsetY", pc.FireOffsetY[0], val => pc.FireOffsetY[0] = val, cmd);

        ImGui.Separator();

        if (data.PowerNames is null)
        {
            ImGui.Text("In order to edit the TrapPowers, import powerTypes.csv");
            ImGui.Spacing();
        }

        // do the commands that change the array's shape at the end
        List<ICommand> arrayAlterCommands = [];
        void changeTrapPowersCommand(string[] newTrapPowers, double[]? newFireOffsetX, double[]? newFireOffsetY)
        {
            arrayAlterCommands.Add(new PropChangeCommand<(string[], double[]?, double[]?)>(val =>
            {
                (string[] trapPowers, double[]? fireOffsetX, double[]? fireOffsetY) = val;
                pc.TrapPowers = trapPowers;
                if (fireOffsetX is not null) pc.FireOffsetX = fireOffsetX;
                if (fireOffsetY is not null) pc.FireOffsetY = fireOffsetY;
            },
            (pc.TrapPowers, fireOffsetXShared ? null : pc.FireOffsetX, fireOffsetYShared ? null : pc.FireOffsetY),
            (newTrapPowers, newFireOffsetX, newFireOffsetY)));

            propChanged = true;
        }

        if (pc.TrapPowers.Length == 0)
            ImGui.TextWrapped(EMPTY_TRAP_POWERS_WARNING);

        ImGuiExt.BeginStyledChild("TrapPowers");
        for (int i = 0; i < pc.TrapPowers.Length; ++i)
        {
            int iCapture = i;
            string trapPower = pc.TrapPowers[i];

            if (ImGuiExt.ButtonDisabledIf(pc.TrapPowers.Length < 2, $"Remove##{i}{trapPower.GetHashCode()}"))
            {
                string[] newTrapPowers = WmeUtils.RemoveAt(pc.TrapPowers, i);
                double[]? newFireOffsetX = fireOffsetXShared ? null : WmeUtils.RemoveAt(pc.FireOffsetX, i);
                double[]? newFireOffsetY = fireOffsetYShared ? null : WmeUtils.RemoveAt(pc.FireOffsetY, i);
                changeTrapPowersCommand(newTrapPowers, newFireOffsetX, newFireOffsetY);
            }

            ImGui.Text($"{trapPower}");
            if (data.PowerNames is not null)
            {
                ImGui.SameLine();
                if (ImGui.Button($"Edit##trappower{i}"))
                    ImGui.OpenPopup(POWER_POPUP_NAME + i);
                propChanged |= PowerEditPopup(data.PowerNames, pc.TrapPowers[i], val => pc.TrapPowers[iCapture] = val, cmd, i.ToString());
            }

            if (!fireOffsetXShared)
            {
                double newOffset = ImGuiExt.DragDouble($"FireOffsetX##{i}", pc.FireOffsetX[i]);
                if (newOffset != pc.FireOffsetX[i])
                {
                    cmd.Add(new PropChangeCommand<double>(val => pc.FireOffsetX[iCapture] = val, pc.FireOffsetX[i], newOffset));
                    propChanged = true;
                }
            }

            if (!fireOffsetYShared)
            {
                double newOffset = ImGuiExt.DragDouble($"FireOffsetY##{i}", pc.FireOffsetY[i]);
                if (newOffset != pc.FireOffsetY[i])
                {
                    cmd.Add(new PropChangeCommand<double>(val => pc.FireOffsetY[iCapture] = val, pc.FireOffsetY[i], newOffset));
                    propChanged = true;
                }
            }

            if (i != pc.TrapPowers.Length - 1)
                ImGui.Separator();
        }
        ImGuiExt.EndStyledChild();

        if (data.PowerNames is not null && ImGui.Button("Add new power"))
        {
            string newTrapPower = data.PowerNames[0];
            string[] newTrapPowers = [.. pc.TrapPowers, newTrapPower];
            double[]? newFireOffsetX = fireOffsetXShared ? null : [.. pc.FireOffsetX, 0];
            double[]? newFireOffsetY = fireOffsetYShared ? null : [.. pc.FireOffsetY, -10];
            changeTrapPowersCommand(newTrapPowers, newFireOffsetX, newFireOffsetY);
        }

        foreach (ICommand command in arrayAlterCommands)
            cmd.Add(command, allowMerge: false);

        return propChanged;
    }

    public static bool ShowLavaCollisionProps(LavaCollision lc, CommandHistory cmd, PropertiesWindowData data)
    {
        bool propChanged = ShowAbstractCollisionProps(lc, cmd, data);

        ImGui.SeparatorText($"Lava collision props##props{lc.GetHashCode()}");
        if (data.PowerNames is null)
        {
            ImGui.Text("In order to edit the LavaPower, import powerTypes.csv");
            ImGui.Spacing();
            ImGui.Text("LavaPower: " + lc.LavaPower);
        }
        else
        {
            ImGui.Text($"LavaPower: {lc.LavaPower}");
            ImGui.SameLine();
            if (ImGui.Button("Edit##lavapower"))
                ImGui.OpenPopup(POWER_POPUP_NAME);
            propChanged |= PowerEditPopup(data.PowerNames, lc.LavaPower, val => lc.LavaPower = val, cmd);
        }

        return propChanged;
    }

    public static C DefaultCollision<C>(double startX, double startY, double endX, double endY) where C : AbstractCollision, new()
    {
        C col = new() { X1 = startX, X2 = endX, Y1 = startY, Y2 = endY };
        if (col is AbstractPressurePlateCollision pcol)
        {
            pcol.AssetName = "a__AnimationPressurePlate";
            pcol.FireOffsetX = [];
            pcol.FireOffsetY = [-10];
            pcol.TrapPowers = ["ClimbTrap"];
            pcol.AnimOffsetX = (col.X1 + col.X2) / 2;
            pcol.AnimOffsetY = (col.Y1 + col.Y2) / 2;
            pcol.Cooldown = 3000;
        }
        if (col is LavaCollision lcol)
        {
            lcol.LavaPower = "LavaBurn";
        }
        return col;
    }

    private static Maybe<AbstractCollision> ShowChangeColTypeMenu(AbstractCollision og)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.Button("Change type"))
            ImGui.OpenPopup("ChangeType##col");

        if (ImGui.BeginPopup("ChangeType##col"))
        {
            result = AddObjectPopup.AddCollisionMenu(og.X1, og.Y1, og.X2, og.Y2);

            // avoid changing type to the same one
            result = result.NoneIf(col => col.GetType() == og.GetType());

            result.DoIfSome(col =>
            {
                col.Parent = og.Parent;
                col.TauntEvent = og.TauntEvent;
                col.Team = og.Team;
                col.AnchorX = og.AnchorX;
                col.AnchorY = og.AnchorY;
                col.NormalX = og.NormalX;
                col.NormalY = og.NormalY;
                col.X1 = og.X1;
                col.X2 = og.X2;
                col.Y1 = og.Y1;
                col.Y2 = og.Y2;
                col.Flag = og.Flag;
                col.ColorFlag = og.ColorFlag;

                if (col is AbstractPressurePlateCollision pcol && og is AbstractPressurePlateCollision pog)
                {
                    pcol.AnimOffsetX = pog.AnimOffsetX;
                    pcol.AnimOffsetY = pog.AnimOffsetY;
                    pcol.AnimRotation = pog.AnimRotation;
                    pcol.AssetName = pog.AssetName;
                    pcol.Cooldown = pog.Cooldown;
                    pcol.FaceLeft = pog.FaceLeft;
                    pcol.FireOffsetX = [.. pog.FireOffsetX];
                    pcol.FireOffsetY = [.. pog.FireOffsetY];
                    pcol.PlatID = pog.PlatID;
                    pcol.TrapPowers = [.. pog.TrapPowers];
                }
            });

            ImGui.EndPopup();
        }
        return result;
    }

    private static double[] NormalizeFireOffset(int trapPowerCount, double[] offsets, double @default)
    {
        if (offsets.Length == 0)
        {
            return [@default];
        }
        else if (offsets.Length != trapPowerCount)
        {
            return [offsets[0]];
        }
        else
        {
            return offsets;
        }
    }

    private static bool FireOffsetModeEdit(string offsetText, int trapPowerCount, double[] offsets, Action<double[]> change, CommandHistory cmd)
    {
        bool shared = offsets.Length == 1;
        // shared (0): all trappowers share the same fire offset
        // seperate (1): all trappowers have an individual fire offset
        int mode = shared ? 0 : 1, oldMode = mode;
        ImGui.Combo($"{offsetText} mode", ref mode, ["shared", "separate"], 2);
        if (mode != oldMode)
        {
            // separate to shared
            if (mode == 0)
            {
                double[] newOffsets = [offsets[0]];
                cmd.Add(new PropChangeCommand<double[]>(change, offsets, newOffsets), allowMerge: false);
                return true;
            }
            // shared to separate
            else
            {
                double[] newOffsets = [.. Enumerable.Repeat(offsets[0], trapPowerCount)];
                cmd.Add(new PropChangeCommand<double[]>(change, offsets, newOffsets), allowMerge: false);
                return true;
            }
        }
        return false;
    }

    private const string POWER_POPUP_NAME = "PowerEdit";
    private static string _powerFilter = "";

    private static bool PowerEditPopup(string[] allPowers, string currentPower, Action<string> change, CommandHistory cmd, string popupId = "")
    {
        bool propChanged = false;
        if (ImGui.BeginPopup(POWER_POPUP_NAME + popupId, ImGuiWindowFlags.NoMove))
        {
            _powerFilter = ImGuiExt.InputText("##powerfilter", _powerFilter, flags: ImGuiInputTextFlags.None);
            if (_powerFilter != "")
            {
                ImGui.SameLine();
                if (ImGui.Button("x")) _powerFilter = "";
            }
            ImGui.SameLine();
            ImGui.Text("Filter");
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Search through all powertypes in the game. Note that not all powers will be compatible with traps/lava and changing this can crash the game.");

            string[] powers = allPowers
                .Where(p => p.Contains(_powerFilter, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            string newPower = ImGuiExt.StringListBox("Power", currentPower, powers, 320.0f);
            if (currentPower != newPower)
            {
                cmd.Add(new PropChangeCommand<string>(change, currentPower, newPower));
                propChanged = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        return propChanged;
    }

    private static AbstractCollision[] GetColParentArray(AbstractCollision c, LevelDesc desc) =>
        c.Parent is null ? desc.Collisions : c.Parent.Children;

    private static Action<AbstractCollision[]> SetColParentArray(AbstractCollision c, LevelDesc desc) =>
        c.Parent is null
            ? val => desc.Collisions = val
            : val => c.Parent.Children = val;
}