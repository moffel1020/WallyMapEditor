using System.Linq;
using ImGuiNET;

namespace WallyMapEditor;

public static class HistoryPanel
{
    private static bool _open = false;
    public static bool Open { get => _open; set => _open = value; }

    // Remove unused parameter 'selection' if it is not part of a shipped public API [WallyMapEditor]
#pragma warning disable IDE0060
    public static void Show(CommandHistory history, SelectionContext selection)
#pragma warning restore IDE0060
    {
        ImGui.Begin("History", ref _open, ImGuiWindowFlags.NoDocking);

        if (ImGui.Button("Undo##history")) history.Undo();
        ImGui.SameLine();
        if (ImGui.Button("Redo##history")) history.Redo();
        ImGui.SameLine();
        if (ImGui.Button("Clear##history")) history.Clear();

        ImGui.BeginDisabled();
        foreach (ICommand cmd in history.Undone.Reverse())
        {
            ImGui.Text(cmd.GetType().Name);
        }
        ImGui.EndDisabled();

        foreach (ICommand cmd in history.Commands)
        {
            ImGui.Text(cmd.GetType().Name);
        }

        ImGui.End();
    }
}