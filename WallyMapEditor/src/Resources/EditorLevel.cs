using System;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public sealed class EditorLevel
{
    public Level Level { get; }
    public SelectionContext Selection { get; }
    public CommandHistory CommandHistory { get; }

    public event EventHandler? CommandHistoryChanged;

    public EditorLevel(Level level)
    {
        Level = level;
        Selection = new();
        CommandHistory = new(Selection);

        CommandHistory.Changed += (obj, data) =>
        {
            CommandHistoryChanged?.Invoke(obj, data);
        };
    }

    public void ResetState()
    {
        Selection.Object = null;
        CommandHistory.Clear();
    }
}