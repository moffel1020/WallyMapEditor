using System;

namespace WallyMapEditor;

// the ctor requires the arguments passed to be correct
// be nice to it
public class ArrayRemoveCommand<T>(Action<T[]> arrayChange, T[] originalArray, T[] removedArray, T removedValue)
    : PropChangeCommand<T[]>(arrayChange, originalArray, removedArray), ISelectCommand where T : notnull
{
    public void ModifyOnUndo(SelectionContext selection) { }

    public void ModifyOnExecute(SelectionContext selection)
    {
        if (selection.Object == (object)removedValue || selection.IsChildOf(removedValue))
            selection.Object = null;
    }
}