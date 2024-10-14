using System;
using System.Collections.Generic;

namespace WallyMapEditor;

public class CommandHistory(SelectionContext selection)
{
    public Stack<ICommand> Commands { get; set; } = new();
    public Stack<ICommand> Undone { get; set; } = new();

    public event EventHandler? Changed;

    public void Add(ICommand cmd, bool? allowMerge = null)
    {
        Changed?.Invoke(this, EventArgs.Empty);

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
        Changed?.Invoke(this, EventArgs.Empty);

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
        Changed?.Invoke(this, EventArgs.Empty);

        if (Undone.TryPop(out ICommand? cmd))
        {
            if (cmd is ISelectCommand d)
                d.ModifyOnExecute(selection);

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