using System;

namespace WallyMapEditor;

public class PropChangeCommand<T>(Action<T> changeAction, T oldVal, T newVal) : ICommand
{
    private readonly Action<T> _action = changeAction;
    private readonly T _oldVal = oldVal;
    private T _newVal = newVal;

    public void Execute() => _action(_newVal);
    public void Undo() => _action(_oldVal);

    public bool Merge(ICommand cmd)
    {
        if (cmd is PropChangeCommand<T> other)
        {
            _newVal = other._newVal;
            return true;
        }

        return false;
    }

    public bool AllowMerge { get; set; } = true;
}