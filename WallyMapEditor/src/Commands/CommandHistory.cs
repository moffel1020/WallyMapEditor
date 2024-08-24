using System.Collections.Generic;

namespace WallyMapEditor;

public class CommandHistory(SelectionContext selection)
{
    public Stack<ICommand> Commands { get; set; } = new();
    public Stack<ICommand> Undone { get; set; } = new();

    public void Add(ICommand cmd)
    {
        cmd.Execute();
        Undone.Clear();

        if (Commands.TryPeek(out ICommand? prev) && prev.AllowMerge && prev.Merge(cmd))
            return;

        if (cmd is IDeselectCommand d && d.DeselectOnExecute(selection))
            selection.Object = null;

        Commands.Push(cmd);
    }

    public void Undo()
    {
        if (Commands.TryPop(out ICommand? prev))
        {
            if (prev is IDeselectCommand d && d.DeselectOnUndo(selection))
                selection.Object = null;

            prev.Undo();
            Undone.Push(prev);
        }
    }

    public void Redo()
    {
        if (Undone.TryPop(out ICommand? cmd))
        {
            if (cmd is IDeselectCommand d && d.DeselectOnExecute(selection))
                selection.Object = null;

            cmd.Execute();
            Commands.Push(cmd);
        }
    }

    public void SetAllowMerge(bool merge)
    {
        if (Commands.TryPeek(out ICommand? prev))
            prev.AllowMerge = merge;
    }

    public void Clear()
    {
        Commands.Clear();
        Undone.Clear();
    }
}