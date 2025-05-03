using System;

namespace WallyMapEditor;

// the ctor requires the arguments passed to be correct
// be nice to it
public class ArrayRemoveCommand<T> : PropChangeCommand<T[]>, ISelectCommand
{
    private readonly T _removedValue;

    public ArrayRemoveCommand(Action<T[]> arrayChange, T[] originalArray, T[] removedArray, T removedValue)
        : base(arrayChange, originalArray, removedArray)
    {
        _removedValue = removedValue;
        AllowMerge = false;
    }

    public void ModifyOnUndo(SelectionContext selection) { }

    public void ModifyOnExecute(SelectionContext selection)
    {
        if (selection.Object == (object?)_removedValue || WmeUtils.IsObjectChildOf(selection.Object, _removedValue))
            selection.Object = null;
    }
}