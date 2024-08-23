namespace WallyMapEditor;

public interface IDeselectCommand : ICommand
{
    public bool ShouldDeselect(SelectionContext selection);
}