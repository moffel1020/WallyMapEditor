using System;

namespace WallyMapEditor;

public class ArrayRemoveCommand<T>(Action<T[]> arrayChange, T[] removedArray, T removedValue)
    : PropChangeCommand<T[]>(arrayChange, [.. removedArray, removedValue], removedArray), ISelectCommand where T : notnull
{
    public void ModifyOnUndo(SelectionContext selection) { }

    public void ModifyOnExecute(SelectionContext selection)
    {
        if (selection.Object == (object)removedValue || selection.IsChildOf(removedValue))
            selection.Object = null;
    }
}