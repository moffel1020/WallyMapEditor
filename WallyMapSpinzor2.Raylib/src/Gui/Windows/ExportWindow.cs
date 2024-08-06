using System;
using System.Numerics;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using ImGuiNET;

using Raylib_cs;

using NativeFileDialogSharp;

using SwfLib.Tags.ActionsTags;

using AbcDisassembler;

using BrawlhallaSwz;

namespace WallyMapSpinzor2.Raylib;

public class ExportWindow(PathPreferences prefs)
{
    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    private string? _descPreview;
    private string? _typePreview;
    private bool _addToLt = false;
    private bool _backup = true;

    private string? _exportError;
    private string? _exportStatus;

    private const int PREVIEW_SIZE = 25;

    private int[] _backupNums = [];
    private string[] _backupDisplayNames = [];
    private int _selectedBackupIndex;
    private bool _refreshListOnOpen = true;

    public void Show(IDrawable? mapData)
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Export", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        if (mapData is null)
        {
            ImGui.Text("No map data open");
            return;
        }

        ImGui.BeginTabBar("exportTabBar", ImGuiTabBarFlags.None);
        if (ImGui.BeginTabItem("Game"))
        {
            if (mapData is Level level) ShowGameExportTab(level);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("LevelDesc"))
        {
            if (mapData is Level level)
                ShowLevelDescExportTab(level.Desc);
            else if (mapData is LevelDesc desc)
                ShowLevelDescExportTab(desc);

            ImGui.EndTabItem();
        }

        if (mapData is Level l)
        {
            if (ImGui.BeginTabItem("LevelType"))
            {
                ShowLevelTypeExportTab(l);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Playlists"))
            {
                ShowPlaylistsExportTab(l);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        if (_exportError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text($"[Error]: {_exportError}");
            ImGui.PopTextWrapPos();
        }

        if (_exportStatus is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text(_exportStatus);
            ImGui.PopTextWrapPos();
        }

        ImGui.End();
    }

    public void ShowGameExportTab(Level l)
    {
        ImGui.Text($"Export {l.Desc.LevelName} to game swz files");
        ImGui.PushTextWrapPos();
        ImGui.Text("This will override the game swz files and you will not be able to play online (even if you changed nothing). To play online again verify integrity of game files");
        ImGui.PopTextWrapPos();
        ImGui.Separator();
        if (ImGui.Button("Select Brawlhalla path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                if (result.IsOk)
                    prefs.BrawlhallaPath = result.Path;
            });
        }
        ImGui.Text($"Path: {prefs.BrawlhallaPath}");

        ImGui.Checkbox("Create backup for swz files", ref _backup);

        if (l.Playlists.Count == 0)
        {
            ImGui.Separator();
            ImGui.TextWrapped("Warning: this level is not in any playlists so it will not be playable in game. Consider adding playlists here:");
            if (ImGui.Button("Add playlists")) PlaylistEditPanel.Open = true;
            ImGui.Separator();
        }

        if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                try
                {
                    ExportToGameSwzFiles(l, _backup);
                    _exportError = null;
                    _exportStatus = null;
                }
                catch (Exception e)
                {
                    Rl.TraceLog(TraceLogLevel.Error, e.Message);
                    Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    _exportError = e.Message;
                    _exportStatus = null;
                }
            });
        }

        if (prefs.BrawlhallaPath is not null && ImGui.CollapsingHeader("Previous backups"))
        {
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

            ImGui.ListBox("Backups", ref _selectedBackupIndex, _backupDisplayNames, _backupDisplayNames.Length);
            if (ImGuiExt.WithDisabledButton(_selectedBackupIndex < 0 || _selectedBackupIndex >= _backupDisplayNames.Length, "Restore"))
            {
                int backupNum = _backupNums[_selectedBackupIndex];

                foreach (string path in backedUpFiles)
                {
                    if (File.Exists(path)) File.Delete(path);
                    File.Move(Wms2RlUtils.CreateBackupPath(path, backupNum), path);
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
                    string backupPath = Wms2RlUtils.CreateBackupPath(path, backupNum);
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }

                RefreshBackupList(prefs.BrawlhallaPath!);
            }

        }
    }

    public void ShowLevelDescExportTab(LevelDesc ld)
    {
        if (ld is null) return;

        ImGui.Text("preview");

        if (_descPreview is not null)
            ImGui.InputTextMultiline("leveldesc##preview", ref _descPreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
        else if (ImGui.Button("Generate preview"))
            _descPreview = Wms2RlUtils.SerializeToString(ld);

        if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                _exportStatus = "exporting...";
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelDescPath));
                if (result.IsOk)
                {
                    Wms2RlUtils.SerializeToPath(ld, result.Path);
                    prefs.LevelDescPath = result.Path;
                    _exportError = null;
                }
                _exportStatus = null;
            });
        }
    }

    public void ShowLevelTypeExportTab(Level l)
    {
        if (l.Type is null) return;

        ImGui.Text("preview");
        if (_typePreview is not null)
            ImGui.InputTextMultiline("leveltype##preview", ref _typePreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
        else if (ImGui.Button("Generate preview"))
            _typePreview = Wms2RlUtils.SerializeToString(l.Type);

        ImGui.Checkbox("Add to LevelTypes.xml", ref _addToLt);
        if (_addToLt)
        {
            if (ImGui.Button("LevelTypes.xml"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelTypesPath));
                    if (result.IsOk)
                    {
                        prefs.LevelTypesPath = result.Path;
                    }
                });
            }
            ImGui.SameLine();
            ImGui.Text(prefs.LevelTypesPath ?? "None");

            if (ImGuiExt.WithDisabledButton(prefs.LevelTypesPath is null, "Export"))
            {
                Task.Run(() =>
                {
                    try
                    {
                        _exportStatus = "exporting...";
                        LevelTypes lts = Wms2RlUtils.DeserializeFromPath<LevelTypes>(prefs.LevelTypesPath!);
                        if (lts.Levels.Length == 0) throw new Exception($"Could not read LevelTypes.xml from given path {prefs.LevelTypesPath}");
                        lts.AddOrUpdateLevelType(l.Type);
                        DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelTypesPath));
                        if (result.IsOk)
                        {
                            Wms2RlUtils.SerializeToPath(lts, result.Path);
                            _exportError = null;
                        }
                        _exportStatus = null;
                    }
                    catch (Exception e)
                    {
                        Rl.TraceLog(TraceLogLevel.Error, e.Message);
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                        _exportError = e.Message;
                        _exportStatus = null;
                    }
                });
            }
        }
        else if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelTypePath));
                if (result.IsOk)
                {
                    _exportStatus = "exporting...";
                    Wms2RlUtils.SerializeToPath(l.Type, result.Path);
                    prefs.LevelTypePath = result.Path;
                    _exportError = null;
                    _exportStatus = null;
                }
            });
        }
    }

    public void ShowPlaylistsExportTab(Level l)
    {
        ImGui.Text($"{l.Desc.LevelName} is in {l.Playlists.Count} playlists");
        if (ImGui.TreeNode("Playlists"))
        {
            foreach (string playlist in l.Playlists)
                ImGui.BulletText(playlist);
            ImGui.TreePop();
        }

        if (ImGui.Button("Add/Remove playlists")) PlaylistEditPanel.Open = true;

        ImGui.Separator();
        ImGui.TextWrapped("Add selection to LevelSetTypes.xml");
        if (ImGui.Button("Select LevelSetTypes.xml"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                if (result.IsOk)
                    prefs.LevelSetTypesPath = result.Path;
            });
        }
        ImGui.SameLine();
        ImGui.Text(prefs.LevelSetTypesPath ?? "Not Selected");

        if (ImGuiExt.WithDisabledButton(string.IsNullOrEmpty(prefs.LevelSetTypesPath), "Export##lst"))
        {
            LevelSetTypes levelSetTypes = Wms2RlUtils.DeserializeFromPath<LevelSetTypes>(prefs.LevelSetTypesPath!);
            UpdatePlaylists(levelSetTypes, l);

            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                if (result.IsOk)
                    Wms2RlUtils.SerializeToPath(levelSetTypes, result.Path);
            });
        }
    }

    public void ExportToGameSwzFiles(Level l, bool backup)
    {
        if (!Wms2RlUtils.IsValidBrawlPath(prefs.BrawlhallaPath))
            throw new InvalidDataException("Selected brawlhalla path is invalid");
        _exportStatus = "finding swz key...";
        if (Wms2RlUtils.GetDoABCDefineTag(Path.Combine(prefs.BrawlhallaPath!, "BrawlhallaAir.swf")) is not DoABCDefineTag abcTag)
            throw new InvalidDataException("Could not find decryption key");
        AbcFile abcFile = AbcFile.Read(abcTag.ABCData);

        uint key = Wms2RlUtils.FindDecryptionKey(abcFile) ?? throw new InvalidDataException("Could not find decryption key");
        prefs.DecryptionKey = key.ToString();
        _exportStatus = "found!";
        string ldData = Wms2RlUtils.SerializeToString(l.Desc, true);

        string dynamicPath = Path.Combine(prefs.BrawlhallaPath!, "Dynamic.swz");
        string initPath = Path.Combine(prefs.BrawlhallaPath!, "Init.swz");
        string gamePath = Path.Combine(prefs.BrawlhallaPath!, "Game.swz");

        if (backup)
        {
            _exportStatus = "creating backup...";
            Wms2RlUtils.CreateBackupOfFile(dynamicPath);
            Wms2RlUtils.CreateBackupOfFile(initPath);
            Wms2RlUtils.CreateBackupOfFile(gamePath);
        }

        _exportStatus = "reading swz...";

        Dictionary<string, string> dynamicFiles = [];
        foreach (string content in Wms2RlUtils.GetFilesInSwz(dynamicPath, key))
            dynamicFiles.Add(SwzUtils.GetFileName(content), content);
        dynamicFiles[SwzUtils.GetFileName(ldData)] = ldData;

        Dictionary<string, string> initFiles = [];
        foreach (string content in Wms2RlUtils.GetFilesInSwz(initPath, key))
            initFiles.Add(SwzUtils.GetFileName(content), content);
        LevelTypes lts = Wms2RlUtils.DeserializeFromString<LevelTypes>(initFiles["LevelTypes.xml"]);
        lts.AddOrUpdateLevelType(l.Type ?? throw new ArgumentNullException("l.Type"));
        initFiles["LevelTypes.xml"] = Wms2RlUtils.SerializeToString(lts, true);

        Dictionary<string, string> gameFiles = [];
        foreach (string content in Wms2RlUtils.GetFilesInSwz(gamePath, key))
            gameFiles.Add(SwzUtils.GetFileName(content), content);
        LevelSetTypes lst = Wms2RlUtils.DeserializeFromString<LevelSetTypes>(gameFiles["LevelSetTypes.xml"]);
        UpdatePlaylists(lst, l);
        gameFiles["LevelSetTypes.xml"] = Wms2RlUtils.SerializeToString(lst, true);

        _exportStatus = "creating new swz...";
        Wms2RlUtils.SerializeSwzFilesToPath(dynamicPath, dynamicFiles.Values, key);
        Wms2RlUtils.SerializeSwzFilesToPath(initPath, initFiles.Values, key);
        Wms2RlUtils.SerializeSwzFilesToPath(gamePath, gameFiles.Values, key);

        RefreshBackupList(prefs.BrawlhallaPath!);
    }

    private static int[] FindBackups(string dir)
    {
        string[] requiredFiles(int num) => [
            Wms2RlUtils.CreateBackupPath(Path.Combine(dir, "Dynamic.swz"), num),
            Wms2RlUtils.CreateBackupPath(Path.Combine(dir, "Init.swz"), num),
            Wms2RlUtils.CreateBackupPath(Path.Combine(dir, "Game.swz"), num),
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

    private void RefreshBackupList(string brawlPath)
    {
        _backupNums = FindBackups(brawlPath);
        _backupDisplayNames = _backupNums.Select(n => $"Backup {n} - {File.GetLastWriteTime(Path.Combine(brawlPath, $"Dynamic_Backup{n}.swz"))}").ToArray();
    }

    public static void UpdatePlaylists(LevelSetTypes levelSetTypes, Level l)
    {
        foreach (string plName in l.Playlists)
        {
            foreach (LevelSetType lst in levelSetTypes.Playlists)
            {
                if (l.Playlists.Contains(lst.LevelSetName))
                {
                    if (!lst.LevelTypes.Contains(l.Desc.LevelName))
                        lst.LevelTypes = [.. lst.LevelTypes, l.Desc.LevelName];
                }
                else
                {
                    lst.LevelTypes = lst.LevelTypes.Where(n => n != l.Desc.LevelName).ToArray();
                }
            }
        }
    }
}