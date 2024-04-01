namespace WallyMapSpinzor2.Raylib;

public interface ICommand
{
    public void Execute();
    public void Undo();

    // return true if merge was successful, else return false 
    public bool Merge(ICommand cmd);

    public bool AllowMerge { get; set; }
}