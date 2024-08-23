using System;

namespace WallyMapEditor;

public class ArrayAddCommand<T>(Action<T[]> arrayChange, T[] array, T toAdd)
    : PropChangeCommand<T[]>(arrayChange, array, [.. array, toAdd]), IDeselectCommand where T : notnull
{
    public bool ShouldDeselect(SelectionContext selection) => selection.Object == (object)toAdd;
}