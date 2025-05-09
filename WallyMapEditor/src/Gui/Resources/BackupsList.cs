using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImGuiNET;

namespace WallyMapEditor;

public partial class BackupsList
{
    // state that is unique to each instance
    public sealed class ExternalState
    {
        public int SelectedBackupIndex { get; set; }
        public bool RefreshListOnOpen { get; set; } = true;
        public string? BackupStatus { get; set; }
    }

    private int[] _backupNums = [];
    private string[] _backupDisplayNames = [];
    private bool _doingBackup = false;

    public void ShowBackupMenu(PathPreferences prefs, ExternalState state)
    {
        if (prefs.BrawlhallaPath is null)
        {
            ImGui.Text("no brawlhalla path selected");
            return;
        }

        if (state.RefreshListOnOpen)
        {
            RefreshBackupList(prefs.BrawlhallaPath);
            state.RefreshListOnOpen = false;
        }

        ReadOnlySpan<string> backedUpFiles = [
            Path.Combine(prefs.BrawlhallaPath, "Dynamic.swz"),
            Path.Combine(prefs.BrawlhallaPath, "Init.swz"),
            Path.Combine(prefs.BrawlhallaPath, "Game.swz"),
            Path.Combine(prefs.BrawlhallaPath, "Engine.swz"),
        ];

        int selectedBackupIndex = state.SelectedBackupIndex;
        ImGui.ListBox("Backups list", ref selectedBackupIndex, _backupDisplayNames, _backupDisplayNames.Length);
        state.SelectedBackupIndex = selectedBackupIndex;

        if (ImGuiExt.ButtonDisabledIf(state.SelectedBackupIndex < 0 || state.SelectedBackupIndex >= _backupDisplayNames.Length, "Restore"))
        {
            int backupNum = _backupNums[state.SelectedBackupIndex];

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
        if (ImGuiExt.ButtonDisabledIf(state.SelectedBackupIndex < 0 || state.SelectedBackupIndex >= _backupDisplayNames.Length, "Delete"))
        {
            int backupNum = _backupNums[state.SelectedBackupIndex];
            foreach (string path in backedUpFiles)
            {
                string backupPath = WmeUtils.CreateBackupPath(path, backupNum);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }

            RefreshBackupList(prefs.BrawlhallaPath);
        }

        ImGui.SameLine();
        if (ImGuiExt.ButtonDisabledIf(_doingBackup, "Create backup"))
        {
            state.BackupStatus = "creating backup...";
            _doingBackup = true;
            string dynamicPath = Path.Combine(prefs.BrawlhallaPath, "Dynamic.swz");
            string initPath = Path.Combine(prefs.BrawlhallaPath, "Init.swz");
            string gamePath = Path.Combine(prefs.BrawlhallaPath, "Game.swz");
            string enginePath = Path.Combine(prefs.BrawlhallaPath, "Engine.swz");
            Task.Run(() =>
            {
                WmeUtils.CreateBackupOfFile(dynamicPath);
                WmeUtils.CreateBackupOfFile(initPath);
                WmeUtils.CreateBackupOfFile(gamePath);
                WmeUtils.CreateBackupOfFile(enginePath);
                RefreshBackupList(prefs.BrawlhallaPath);
                _doingBackup = false;
                state.BackupStatus = null;
            });
        }

        if (state.BackupStatus is not null)
        {
            ImGui.Text(state.BackupStatus);
        }
    }

    private static IEnumerable<int> FindBackups(string dir)
    {
        string[] requiredFiles(int num) => [
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Dynamic.swz"), num),
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Init.swz"), num),
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Game.swz"), num),
            WmeUtils.CreateBackupPath(Path.Combine(dir, "Engine.swz"), num),
        ];

        Regex re = BackupFileRegex();

        return Directory.EnumerateFiles(dir)
            .Where(p => p.Contains("_Backup"))
            .MapFilter(p =>
            {
                string name = Path.GetFileNameWithoutExtension(p);

                Match match = re.Match(name);
                if (!match.Success)
                    return Maybe<int>.None;

                string backupNumString = match.Groups[1].Value;
                if (!int.TryParse(backupNumString, out int backupNum))
                    return Maybe<int>.None;

                return backupNum;
            })
            .Distinct()
            .Where(num => requiredFiles(num).All(File.Exists));
    }

    public void RefreshBackupList(string brawlPath)
    {
        _backupNums = [.. FindBackups(brawlPath)];
        _backupDisplayNames = [.. _backupNums.Select(n => $"Backup {n} - {File.GetLastWriteTime(Path.Combine(brawlPath, $"Dynamic_Backup{n}.swz"))}")];
    }

    [GeneratedRegex(@"^(?:.*)_Backup([0-9]+)$")]
    private static partial Regex BackupFileRegex();
}