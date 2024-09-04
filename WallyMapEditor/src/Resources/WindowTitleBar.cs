namespace WallyMapEditor;

public class WindowTitleBar
{
    public const string WINDOW_NAME = "WallyMapEditor";

    private string _fullTitle = WINDOW_NAME;
    public string FullTitle
    {
        get => _fullTitle;
        private set
        {
            _fullTitle = value;
            Rl.SetWindowTitle(value);
        }
    }

    public string? OpenLevelFile { get; private set; }
    public bool Unsaved { get; private set; }

    public void SetTitle(string? openFile, bool unsaved)
    {
        if (openFile == OpenLevelFile && unsaved == Unsaved) return;

        (OpenLevelFile, Unsaved) = (openFile, unsaved);

        string title = WINDOW_NAME;
        if (OpenLevelFile is not null) title += " - " + (unsaved ? "*" : "") + openFile;

        FullTitle = title;
    }

    public void Reset() => FullTitle = WINDOW_NAME;
}