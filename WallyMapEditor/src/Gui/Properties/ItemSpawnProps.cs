using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowItemSpawnProps(AbstractItemSpawn i, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;
        SelectionContext selection = level.Selection;
        LevelDesc ld = level.Level.Desc;

        if (i.Parent is not null)
        {
            ImGui.Text($"Parent DynamicItemSpawn: ");
            ImGui.SameLine();
            if (ImGui.Button($"PlatID {i.Parent.PlatID}"))
                selection.Object = i.Parent;
            ImGui.Separator();
        }

        RemoveButton(i, level);
        ImGui.Separator();

        bool propChanged = false;

        propChanged |= WmeUtils.ObjectChangeType(i, ld, cmd, ShowChangeItemTypeMenu);
        propChanged |= ImGuiExt.DragDoubleHistory($"X##props{i.GetHashCode()}", i.X, val => i.X = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"Y##props{i.GetHashCode()}", i.Y, val => i.Y = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"W##props{i.GetHashCode()}", i.W, val => i.W = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory($"H##props{i.GetHashCode()}", i.H, val => i.H = val, cmd);
        return propChanged;
    }

    private static Maybe<AbstractItemSpawn> ShowChangeItemTypeMenu(AbstractItemSpawn og)
    {
        Maybe<AbstractItemSpawn> result = new();
        if (ImGui.Button("Change Type"))
            ImGui.OpenPopup("ChangeType##item");

        if (ImGui.BeginPopup("ChangeType##item"))
        {
            result = AddObjectPopup.AddItemSpawnMenu(og.X, og.Y).NoneIf(i => i.GetType() == og.GetType());

            result.DoIfSome(item =>
            {
                item.Parent = og.Parent;
                item.H = og.H;
                item.W = og.W;
                item.X = og.X;
                item.Y = og.Y;
            });

            ImGui.EndPopup();
        }

        return result;
    }

    public static T DefaultItemSpawn<T>(double posX, double posY) where T : AbstractItemSpawn, new()
    {
        T spawn = new()
        {
            X = posX,
            Y = posY
        };
        (spawn.W, spawn.H) = (spawn.DefaultW, spawn.DefaultH);
        if (spawn is ItemSpawn)
            spawn.W = 100;
        return spawn;
    }
}