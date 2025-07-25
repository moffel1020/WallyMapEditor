using System;
using System.Linq;
using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, EditorLevel level, PropertiesWindowData data)
        where T : IDeserializable, ISerializable, IDrawable
    {
        CommandHistory cmd = level.CommandHistory;
        SelectionContext selection = level.Selection;
        LevelDesc ld = level.Level.Desc;

        bool propChanged = false;

        ShowPlatIDEdit(val => ad.PlatID = val, ad.PlatID, level, cmd);

        ImGui.Separator();
        RemoveButton(ad, level);
        ImGui.Separator();

        propChanged |= ImGuiExt.DragDoubleHistory("X", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("Y", ad.Y, val => ad.Y = val, cmd);

        if (ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", ad.Children, val => ad.Children = val,
            () => CreateDynamicChild(ad, ld),
            index =>
            {
                bool changed = false;
                if (index >= ad.Children.Length) return false;
                T child = ad.Children[index];
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, level, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##dyncol{child.GetHashCode()}"))
                    selection.Object = child;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }

        return propChanged;
    }

    private static string[] GetKnownPlatIDs(EditorLevel level)
        => level.Level.Desc.Assets.OfType<MovingPlatform>().Select(mp => mp.PlatID).ToArray() ?? [];

    public static bool ShowNullablePlatIDEdit(Action<string?> changeCommand, string? value, EditorLevel level, CommandHistory cmd)
    {
        bool propChanged = false;
        if (value is not null)
        {
            propChanged = ShowPlatIDEdit(changeCommand, value, level, cmd);
            if (ImGui.Button("Remove PlatID"))
            {
                cmd.Add(new PropChangeCommand<string?>(changeCommand, value, null), false);
                propChanged = true;
            }
        }
        else if (ImGui.Button("Add PlatID"))
        {
            cmd.Add(new PropChangeCommand<string?>(changeCommand, value, "0"), false);
            propChanged = true;
        }

        return propChanged;
    }

    public static bool ShowPlatIDEdit(Action<string> changeCommand, string value, EditorLevel level, CommandHistory cmd)
    {
        SelectionContext selection = level.Selection;
        LevelDesc ld = level.Level.Desc;

        string[] knownPlatIds = GetKnownPlatIDs(level);
        if (knownPlatIds.Contains(value))
        {
            ImGui.Text("Animated by MovingPlatform");
            ImGui.SameLine();
            if (ImGui.Button($"({value})"))
                selection.Object = ld.Assets.OfType<MovingPlatform>().Last(mp => mp.PlatID == value);
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
                        cmd.Add(new PropChangeCommand<string>(changeCommand, value, id), false);
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
            Respawn child = DefaultRespawn(0, 0);
            child.Parent = parent;
            result = child;
        }
        return result;
    }

    private static Maybe<NavNode> CreateNavNodeChild(DynamicNavNode parent, LevelDesc desc)
    {
        Maybe<NavNode> result = new();
        if (ImGui.Button("Add new navnode"))
        {
            NavNode child = DefaultNavNode(0, 0, desc);
            child.Parent = parent;
            result = child;
        }
        return result;
    }
}