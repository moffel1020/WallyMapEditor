using System;

namespace WallyMapEditor;

public class PropChangeCommand<T>(Action<T> changeAction, T oldVal, T newVal) : ICommand
{
    protected Action<T> Action { get; init; } = changeAction;
    protected T OldVal { get; init; } = oldVal;
    protected T NewVal { get; set; } = newVal;

    public void Execute() => Action(NewVal);
    public void Undo() => Action(OldVal);

    public bool Merge(ICommand cmd)
    {
        if (cmd is PropChangeCommand<T> other)
        {
            NewVal = other.NewVal;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}