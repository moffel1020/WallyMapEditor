namespace WallyMapEditor;

public interface ISelectCommand : ICommand
{
    public void ModifyOnUndo(SelectionContext selection);
    public void ModifyOnExecute(SelectionContext selection);
}