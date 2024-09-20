using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImGuiNET;

namespace WallyMapEditor;

public class BackupsWindow(PathPreferences prefs)
{
    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    private int[] _backupNums = [];
    private string[] _backupDisplayNames = [];
    private int _selectedBackupIndex;
    private bool _refreshListOnOpen = true;

    private string? _backupStatus;
    private bool _doingBackup = false;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Backups", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        ShowBackupMenu();
        ImGui.End();
    }

    public void ShowBackupMenu()
    {
        if (prefs.BrawlhallaPath is null)
        {
            ImGui.Text("no brawlhalla path selected");
            return;
        }

        if (_refreshListOnOpen)
        {
            RefreshBackupList(prefs.BrawlhallaPath);
            _refreshListOnOpen = false;
        }

        string[] backedUpFiles = [
            Path.Combine(prefs.BrawlhallaPath, "Dynamic.swz"),
                Path.Combine(prefs.BrawlhallaPath, "Init.swz"),
                Path.Combine(prefs.BrawlhallaPath, "Game.swz")
        ];

        ImGui.ListBox("Backups list", ref _selectedBackupIndex, _backupDisplayNames, _backupDisplayNames.Length);

        if (ImGuiExt.WithDisabledButton(_selectedBackupIndex < 0 || _selectedBackupIndex >= _backupDisplayNames.Length, "Restore"))
        {
            int backupNum = _backupNums[_selectedBackupIndex];

            foreach (string path in backedUpFiles)
            {
                if (File.Exists(path)) File.Delete(path);
                File.Move(WmeUtils.CreateBackupPath(path, backupNum), path);
            }

            RefreshBackupList(prefs.BrawlhallaPath);
        }

        ImGui.SameLine();
        if (ImGui.Button("Refresh"))
            RefreshBackupList(prefs.BrawlhallaPath);

        ImGui.SameLine();
        if (ImGuiExt.WithDisabledButton(_selectedBackupIndex < 0 || _selectedBackupIndex >= _backupDisplayNames.Length, "Delete"))
        {
            int backupNum = _backupNums[_selectedBackupIndex];
            foreach (string path in backedUpFiles)
            {
                string backupPath = WmeUtils.CreateBackupPath(path, backupNum);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }

            RefreshBackupList(prefs.BrawlhallaPath);
        }

        ImGui.SameLine();
        if (ImGuiExt.WithDisabledButton(_doingBackup, "Create backup"))
        {
            string dynamicPath = Path.Combine(prefs.BrawlhallaPath, "Dynamic.swz");
            string initPath = Path.Combine(prefs.BrawlhallaPath, "Init.swz");
            string gamePath = Path.Combine(prefs.BrawlhallaPath, "Game.swz");
            _backupStatus = "creating backup...";
            _doingBackup = true;
            Task.Run(() =>
            {
                WmeUtils.CreateBackupOfFile(dynamicPath);
                WmeUtils.CreateBackupOfFile(initPath);
                WmeUtils.CreateBackupOfFile(gamePath);
                _backupStatus = null;
                _doingBackup = false;
                RefreshBackupList(prefs.BrawlhallaPath);
            });
        }

        if (_backupStatus is not null)
        {
            ImGui.Text(_backupStatus);
        }
    }

    private static int[] FindBackups(string dir)
    {
        string[] requiredFiles(int num) => [
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Dynamic.swz"), num),
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Init.swz"), num),
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Game.swz"), num),
        ];

        int[] validBackupNumbers = Directory.EnumerateFiles(dir)
            .Where(p => p.Contains("_Backup"))
            .Select(p => Path.GetFileNameWithoutExtension(p).Split("_Backup").Last())
            .MapFilter(n => int.TryParse(n, out int i) ? i : Maybe<int>.None)
            .Distinct()
            .Where(num => requiredFiles(num).All(File.Exists))
            .ToArray();

        return validBackupNumbers;
    }

    public void RefreshBackupList(string brawlPath)
    {
        _backupNums = FindBackups(brawlPath);
        _backupDisplayNames = _backupNums.Select(n => $"Backup {n} - {File.GetLastWriteTime(Path.Combine(brawlPath, $"Dynamic_Backup{n}.swz"))}").ToArray();
    }
}