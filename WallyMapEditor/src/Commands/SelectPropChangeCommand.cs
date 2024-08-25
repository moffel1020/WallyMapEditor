using System;

namespace WallyMapEditor;

public class SelectPropChangeCommand<T>(Action<T> changeAction, T oldVal, T newVal)
    : PropChangeCommand<T>(changeAction, oldVal, newVal), ISelectCommand
    where T : notnull
{
    public void ModifyOnExecute(SelectionContext selection)
    {
        if (selection.Object == (object)OldVal)
            selection.Object = NewVal;
    }

    public void ModifyOnUndo(SelectionContext selection)
    {
        if (selection.Object == (object)NewVal)
            selection.Object = OldVal;
    }
}