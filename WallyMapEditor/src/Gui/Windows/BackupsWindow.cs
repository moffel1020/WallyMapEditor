using ImGuiNET;

namespace WallyMapEditor;

public class BackupsWindow(PathPreferences prefs, BackupsList backups)
{
    private BackupsList.ExternalState _state = new();

    private bool _open;
    public bool Open
    {
        get => _open; set
        {
            _open = value;
            if (!_open) _state = new();
        }
    }


    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Backups", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        backups.ShowBackupMenu(prefs, _state);
        ImGui.End();
    }
}