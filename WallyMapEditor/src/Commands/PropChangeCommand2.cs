using System;

namespace WallyMapEditor;

public class PropChangeCommand<T1, T2>(Action<T1, T2> changeAction, T1 oldVal1, T2 oldVal2, T1 newVal1, T2 newVal2) : ICommand
{
    protected Action<T1, T2> Action { get; init; } = changeAction;
    protected T1 OldVal1 { get; init; } = oldVal1;
    protected T2 OldVal2 { get; init; } = oldVal2;
    protected T1 NewVal1 { get; set; } = newVal1;
    protected T2 NewVal2 { get; set; } = newVal2;

    public void Execute() => Action(NewVal1, NewVal2);
    public void Undo() => Action(OldVal1, NewVal2);

    public bool Merge(ICommand cmd)
    {
        if (cmd is PropChangeCommand<T1, T2> other)
        {
            NewVal1 = other.NewVal1;
            NewVal2 = other.NewVal2;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}