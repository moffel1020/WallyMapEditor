using System;

namespace WallyMapEditor;

public class PropChangeCommand<T1, T2, T3, T4, T5, T6>(
    Action<T1, T2, T3, T4, T5, T6> changeAction,
    T1 oldVal1, T2 oldVal2, T3 oldVal3, T4 oldVal4, T5 oldVal5, T6 oldVal6,
    T1 newVal1, T2 newVal2, T3 newVal3, T4 newVal4, T5 newVal5, T6 newVal6
) : ICommand
{
    protected Action<T1, T2, T3, T4, T5, T6> Action { get; } = changeAction;
    protected T1 OldVal1 { get; } = oldVal1;
    protected T2 OldVal2 { get; } = oldVal2;
    protected T3 OldVal3 { get; } = oldVal3;
    protected T4 OldVal4 { get; } = oldVal4;
    protected T5 OldVal5 { get; } = oldVal5;
    protected T6 OldVal6 { get; } = oldVal6;
    protected T1 NewVal1 { get; set; } = newVal1;
    protected T2 NewVal2 { get; set; } = newVal2;
    protected T3 NewVal3 { get; set; } = newVal3;
    protected T4 NewVal4 { get; set; } = newVal4;
    protected T5 NewVal5 { get; set; } = newVal5;
    protected T6 NewVal6 { get; set; } = newVal6;

    public void Execute() => Action(NewVal1, NewVal2, NewVal3, NewVal4, NewVal5, NewVal6);
    public void Undo() => Action(OldVal1, OldVal2, OldVal3, OldVal4, OldVal5, OldVal6);

    public bool Merge(ICommand cmd)
    {
        if (cmd is PropChangeCommand<T1, T2, T3, T4, T5, T6> other)
        {
            NewVal1 = other.NewVal1;
            NewVal2 = other.NewVal2;
            NewVal3 = other.NewVal3;
            NewVal4 = other.NewVal4;
            NewVal5 = other.NewVal5;
            NewVal6 = other.NewVal6;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}