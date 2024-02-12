namespace WallyMapSpinzor2.Raylib;

public interface IWindow
{
    public void Show();
    public bool Focussed { get; set; }
    public bool Hovered { get; set; }
    public bool Open { get; set; }
}