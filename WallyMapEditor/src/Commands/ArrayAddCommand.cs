using System;

namespace WallyMapEditor;

public class ArrayAddCommand<T>(Action<T[]> arrayChange, T[] array, T toAdd)
    : PropChangeCommand<T[]>(arrayChange, array, [.. array, toAdd]), ISelectCommand where T : notnull
{
    public void ModifyOnUndo(SelectionContext selection)
    {
        if (selection.Object == (object)toAdd || selection.IsChildOf(toAdd))
            selection.Object = null;
    }

    public void ModifyOnExecute(SelectionContext selection) { }
}