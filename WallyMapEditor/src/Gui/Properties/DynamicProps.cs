using System;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, CommandHistory cmd, PropertiesWindowData data)
        where T : IDeserializable, ISerializable, IDrawable
    {
        bool propChanged = false;

        ShowPlatIDEdit(val => ad.PlatID = val, ad.PlatID, data, cmd);

        ImGui.Separator();
        if (data.Level is not null) ShowDynamicRemoveButton(ad, data.Level.Desc, cmd);
        ImGui.Separator();

        propChanged |= ImGuiExt.DragDoubleHistory("X", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", ad.Y, val => ad.Y = val, cmd);

        if (ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", ad.Children, val => ad.Children = val,
            () => CreateDynamicChild(ad, data.Level?.Desc),
            (int index) =>
            {
                bool changed = false;
                if (index >= ad.Children.Length) return false;
                T child = ad.Children[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##dyncol{child.GetHashCode()}")) data.Selection.Object = child;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }

        return propChanged;
    }

    private static string[] GetKnownPlatIDs(PropertiesWindowData data)
        => data.Level?.Desc.Assets.OfType<MovingPlatform>().Select(mp => mp.PlatID).ToArray() ?? [];

    public static bool ShowNullablePlatIDEdit(Action<string?> changeCommand, string? value, PropertiesWindowData data, CommandHistory cmd)
    {
        bool propChanged = false;
        if (value is not null)
        {
            propChanged = ShowPlatIDEdit(changeCommand, value, data, cmd);
            if (ImGui.Button("Remove PlatID"))
            {
                cmd.Add(new PropChangeCommand<string?>(changeCommand, value, null));
                cmd.SetAllowMerge(false);
                propChanged = true;
            }
        }
        else if (ImGui.Button("Add PlatID"))
        {
            cmd.Add(new PropChangeCommand<string?>(changeCommand, value, "0"));
            cmd.SetAllowMerge(false);
            propChanged = true;
        }

        return propChanged;
    }

    public static bool ShowPlatIDEdit(Action<string> changeCommand, string value, PropertiesWindowData data, CommandHistory cmd)
    {
        string[] knownPlatIds = GetKnownPlatIDs(data);

        if (data.Level is not null && knownPlatIds.Contains(value))
        {
            ImGui.Text("Animated by MovingPlatform");
            ImGui.SameLine();
            if (ImGui.Button($"({value})"))
                data.Selection.Object = data.Level.Desc.Assets.OfType<MovingPlatform>().Last(mp => mp.PlatID == value);
        }

        bool propChanged = ImGuiExt.InputTextHistory("##platid", value, changeCommand, cmd);
        if (knownPlatIds.Length > 0)
        {
            ImGui.SameLine();
            if (ImGui.BeginCombo("##platidselect", value, ImGuiComboFlags.NoPreview | ImGuiComboFlags.PopupAlignLeft))
            {
                foreach (string id in knownPlatIds)
                {
                    if (ImGui.Selectable(id, id == value))
                    {
                        cmd.Add(new PropChangeCommand<string>(changeCommand, value, id));
                        cmd.SetAllowMerge(false);
                        propChanged = true;
                    }
                }
                ImGui.EndCombo();
            }
        }
        ImGui.SameLine();
        ImGui.Text("PlatID");

        return propChanged;
    }

    private static Maybe<T> CreateDynamicChild<T>(AbstractDynamic<T> parent, LevelDesc? ld)
        where T : IDeserializable, ISerializable, IDrawable => parent switch
        {
            DynamicCollision col => CreateCollisionChild(col).Cast<T>(),
            DynamicItemSpawn item => CreateItemSpawnChild(item).Cast<T>(),
            DynamicRespawn res => CreateRespawnChild(res).Cast<T>(),
            DynamicNavNode n when ld is not null => CreateNavNodeChild(n, ld).Cast<T>(),
            _ => Maybe<T>.None
        };

    private static Maybe<AbstractCollision> CreateCollisionChild(DynamicCollision parent)
    {
        Maybe<AbstractCollision> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddCollisionMenu(0, 0, 100, 0);
            result.DoIfSome(col => col.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<AbstractItemSpawn> CreateItemSpawnChild(DynamicItemSpawn parent)
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##dynamic");

        if (ImGui.BeginPopup("AddChild##dynamic"))
        {
            result = AddObjectPopup.AddItemSpawnMenu(0, 0);
            result.DoIfSome(col => col.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }

    private static Maybe<Respawn> CreateRespawnChild(DynamicRespawn parent)
    {
        Maybe<Respawn> result = new();
        if (ImGui.Button("Add new respawn"))
        {
            result = DefaultRespawn(0, 0);
            result.ValueUnsafe.Parent = parent;
        }
        return result;
    }

    private static Maybe<NavNode> CreateNavNodeChild(DynamicNavNode parent, LevelDesc desc)
    {
        Maybe<NavNode> result = new();
        if (ImGui.Button("Add new navnode"))
        {
            result = DefaultNavNode(0, 0, desc);
            result.ValueUnsafe.Parent = parent;
        }
        return result;
    }

    private static bool ShowDynamicRemoveButton<T>(AbstractDynamic<T> ad, LevelDesc desc, CommandHistory cmd)
        where T : IDrawable, IDeserializable, ISerializable => ad switch
        {
            DynamicCollision dc => RemoveButton(dc, cmd, desc.DynamicCollisions, val => desc.DynamicCollisions = val),
            DynamicItemSpawn di => RemoveButton(di, cmd, desc.DynamicItemSpawns, val => desc.DynamicItemSpawns = val),
            DynamicRespawn dr => RemoveButton(dr, cmd, desc.DynamicRespawns, val => desc.DynamicRespawns = val),
            DynamicNavNode dn => RemoveButton(dn, cmd, desc.DynamicNavNodes, val => desc.DynamicNavNodes = val),
            _ => throw new Exception("Could not show remove button for dynamics. Unimplemented dynamic type")
        };
}