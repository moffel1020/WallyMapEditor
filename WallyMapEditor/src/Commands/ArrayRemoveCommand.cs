using System;

namespace WallyMapEditor;

public class ArrayRemoveCommand<T>(Action<T[]> arrayChange, T[] removedArray, T removedValue)
    : PropChangeCommand<T[]>(arrayChange, [.. removedArray, removedValue], removedArray), IDeselectCommand where T : notnull
{
    public bool DeselectOnUndo(SelectionContext selection) => false;
    public bool DeselectOnExecute(SelectionContext selection) => selection.Object == (object)removedValue || selection.IsChildOf(removedValue);
}