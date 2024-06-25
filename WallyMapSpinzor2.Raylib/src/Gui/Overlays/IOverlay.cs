namespace WallyMapSpinzor2.Raylib;

public interface IOverlay
{
    public bool Update(OverlayData data, CommandHistory cmd);
    public void Draw(OverlayData data);
}
