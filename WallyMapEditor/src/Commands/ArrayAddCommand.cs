using System;

namespace WallyMapEditor;

public class ArrayAddCommand<T> : PropChangeCommand<T[]>, ISelectCommand
{
    private readonly T _toAdd;

    public ArrayAddCommand(Action<T[]> arrayChange, T[] array, T toAdd)
        : base(arrayChange, array, [.. array, toAdd])
    {
        _toAdd = toAdd;
        AllowMerge = false;
    }

    public void ModifyOnUndo(SelectionContext selection)
    {
        if (selection.Object == (object?)_toAdd || WmeUtils.IsObjectChildOf(selection.Object, _toAdd))
            selection.Object = null;
    }

    public void ModifyOnExecute(SelectionContext selection) { }
}