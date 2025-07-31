using System.Collections.Generic;

namespace WallyMapEditor;

public sealed class CommandHistory(SelectionContext selection)
{
    private int _saveAtCommands = 0;
    public Stack<ICommand> Commands { get; set; } = new();
    public Stack<ICommand> Undone { get; set; } = new();

    public bool IsSaved => Commands.Count == _saveAtCommands;

    public void Add(ICommand cmd, bool? allowMerge = null)
    {
        cmd.Execute();
        Undone.Clear();

        if (Commands.TryPeek(out ICommand? prev) && prev.AllowMerge && prev.Merge(cmd))
            return;

        if (cmd is ISelectCommand d)
            d.ModifyOnExecute(selection);

        Commands.Push(cmd);

        if (allowMerge is not null)
            SetAllowMerge(allowMerge.Value);
    }

    public void Undo()
    {
        if (Commands.TryPop(out ICommand? prev))
        {
            if (prev is ISelectCommand d)
                d.ModifyOnUndo(selection);

            prev.Undo();
            Undone.Push(prev);
        }
    }

    public void Redo()
    {
        if (Undone.TryPop(out ICommand? cmd))
        {
            if (cmd is ISelectCommand d)
                d.ModifyOnExecute(selection);

            cmd.Execute();
            Commands.Push(cmd);
        }
    }

    public void OnSave()
    {
        _saveAtCommands = Commands.Count;
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
        _saveAtCommands = IsSaved ? 0 : -1;
    }
}