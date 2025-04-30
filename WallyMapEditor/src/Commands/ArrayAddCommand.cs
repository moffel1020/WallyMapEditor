using System;

namespace WallyMapEditor;

public class ArrayAddCommand<T>(Action<T[]> arrayChange, T[] array, T toAdd)
    : PropChangeCommand<T[]>(arrayChange, array, [.. array, toAdd]), ISelectCommand
{
    public void ModifyOnUndo(SelectionContext selection)
    {
        if (selection.Object == (object?)toAdd || WmeUtils.IsObjectChildOf(selection.Object, toAdd))
            selection.Object = null;
    }

    public void ModifyOnExecute(SelectionContext selection) { }

    public override bool AllowMerge => false;
}