namespace WallyMapEditor;

public interface IOverlay
{
    public bool Update(EditorLevel level, OverlayData data);
    public void Draw(EditorLevel level, OverlayData data);
}
