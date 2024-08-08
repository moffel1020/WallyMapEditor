using System.Linq;
using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public static class HistoryPanel
{
    private static bool _open = false;
    public static bool Open { get => _open; set => _open = value; }

    public static void Show(CommandHistory history)
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