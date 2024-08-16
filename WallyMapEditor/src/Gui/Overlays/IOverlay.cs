namespace WallyMapEditor;

public interface IOverlay
{
    public bool Update(OverlayData data, CommandHistory cmd);
    public void Draw(OverlayData data);
}
