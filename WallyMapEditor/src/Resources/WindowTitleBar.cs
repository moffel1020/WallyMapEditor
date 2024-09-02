namespace WallyMapEditor;

public class WindowTitleBar
{
    public const string WINDOW_NAME = "WallyMapEditor";

    private string _fullTitle = WINDOW_NAME;
    public string FullTitle
    {
        get => _fullTitle;
        set
        {
            _fullTitle = value;
            Rl.SetWindowTitle(value);
        }
    }

    private string? _openLevelFile = null;
    public string? OpenLevelFile
    {
        get => _openLevelFile;
        set => (_openLevelFile, FullTitle) = (value, WINDOW_NAME + (value is null ? null : " - " + value));
    }
}