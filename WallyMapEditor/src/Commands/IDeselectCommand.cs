namespace WallyMapEditor;

public interface IDeselectCommand : ICommand
{
    public bool DeselectOnUndo(SelectionContext selection);
    public bool DeselectOnExecute(SelectionContext selection);
}