namespace WallyMapSpinzor2.Raylib;

public interface IDialog
{
    public bool Closed { get; }
    public void Show();
}