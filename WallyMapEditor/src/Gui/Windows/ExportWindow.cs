using System;
using System.Numerics;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using WallyMapSpinzor2;
using BrawlhallaSwz;

using ImGuiNET;
using Raylib_cs;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public class ExportWindow(PathPreferences prefs, BackupsList backups)
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

    private readonly BackupsList.ExternalState _state = new();

    public void Show(Level? level)
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Export", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        if (level is null)
        {
            ImGui.Text("No map data open");
            ImGui.End();
            return;
        }

        ImGui.BeginTabBar("exportTabBar", ImGuiTabBarFlags.None);
        if (ImGui.BeginTabItem("Game"))
        {
            ShowGameExportTab(level);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("LevelDesc"))
        {
            ShowLevelDescExportTab(level.Desc);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("LevelType"))
        {
            ShowLevelTypeExportTab(level);
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("LevelSetTypes"))
        {
            ShowPlaylistsExportTab(level);
            ImGui.EndTabItem();
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

    private void ShowGameExportTab(Level l)
    {
        ImGui.Text($"Export {l.Desc.LevelName} to game swz files");
        ImGui.TextWrapped("This will override the game swz files and you will not be able to play online (even if you changed nothing). To play online again restore from backup or if that doesn't work verify integrity of game files");
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
            if (ImGui.Button("Add default playlists")) l.Playlists = [.. LevelLoader.DefaultPlaylists];
            ImGui.Text("or");
            if (ImGui.Button("Edit playlists manually")) PlaylistEditPanel.Open = true;
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
            backups.ShowBackupMenu(prefs, _state);
        }
    }

    private void ShowLevelDescExportTab(LevelDesc ld)
    {
        if (ld is null) return;

        ImGui.Text("preview");

        if (_descPreview is not null)
            ImGui.InputTextMultiline("leveldesc##preview", ref _descPreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE), ImGuiInputTextFlags.ReadOnly);
        else if (ImGui.Button("Generate preview"))
            _descPreview = WmeUtils.SerializeToString(ld, bhstyle: true);

        if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                _exportStatus = "exporting...";
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelDescPath));
                if (result.IsOk)
                {
                    WmeUtils.SerializeToPath(ld, result.Path, bhstyle: true);
                    prefs.LevelDescPath = result.Path;
                    _exportError = null;
                }
                _exportStatus = null;
            });
        }
    }

    private void ShowLevelTypeExportTab(Level l)
    {
        if (l.Type is null) return;

        ImGui.Text("preview");
        if (_typePreview is not null)
            ImGui.InputTextMultiline("leveltype##preview", ref _typePreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE), ImGuiInputTextFlags.ReadOnly);
        else if (ImGui.Button("Generate preview"))
            _typePreview = WmeUtils.SerializeToString(l.Type, bhstyle: true);

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

            if (ImGuiExt.ButtonDisabledIf(prefs.LevelTypesPath is null, "Export"))
            {
                Task.Run(() =>
                {
                    try
                    {
                        _exportStatus = "exporting...";
                        LevelTypes lts = WmeUtils.DeserializeFromPath<LevelTypes>(prefs.LevelTypesPath!, bhstyle: true);
                        if (lts.Levels.Length == 0) throw new Exception($"Could not read LevelTypes.xml from given path {prefs.LevelTypesPath}");
                        lts.AddOrUpdateLevelType(l.Type);
                        DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelTypesPath));
                        if (result.IsOk)
                        {
                            WmeUtils.SerializeToPath(lts, result.Path, bhstyle: true);
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
                    WmeUtils.SerializeToPath(l.Type, result.Path, bhstyle: true);
                    prefs.LevelTypePath = result.Path;
                    _exportError = null;
                    _exportStatus = null;
                }
            });
        }
    }

    private void ShowPlaylistsExportTab(Level l)
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

        if (ImGuiExt.ButtonDisabledIf(string.IsNullOrEmpty(prefs.LevelSetTypesPath), "Export##lst"))
        {
            LevelSetTypes levelSetTypes = WmeUtils.DeserializeFromPath<LevelSetTypes>(prefs.LevelSetTypesPath!, bhstyle: true);
            UpdatePlaylists(levelSetTypes, l);

            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                if (result.IsOk)
                    WmeUtils.SerializeToPath(levelSetTypes, result.Path, bhstyle: true);
            });
        }
    }

    private void ExportToGameSwzFiles(Level l, bool backup)
    {
        if (!WmeUtils.IsValidBrawlPath(prefs.BrawlhallaPath))
            throw new InvalidDataException("Selected brawlhalla path is invalid");
        _exportStatus = "finding swz key...";

        uint key = WmeUtils.FindDecryptionKeyFromPath(prefs.BrawlhallaAirPath!) ?? throw new InvalidDataException("Could not find decryption key");
        prefs.DecryptionKey = key.ToString();
        _exportStatus = "found!";
        string ldData = WmeUtils.SerializeToString(l.Desc, minify: true, bhstyle: true);

        string dynamicPath = Path.Combine(prefs.BrawlhallaPath!, "Dynamic.swz");
        string initPath = Path.Combine(prefs.BrawlhallaPath!, "Init.swz");
        string gamePath = Path.Combine(prefs.BrawlhallaPath!, "Game.swz");
        string enginePath = Path.Combine(prefs.BrawlhallaPath!, "Engine.swz");

        if (backup)
        {
            _exportStatus = "creating backup...";
            WmeUtils.CreateBackupOfFile(dynamicPath);
            WmeUtils.CreateBackupOfFile(initPath);
            WmeUtils.CreateBackupOfFile(gamePath);
            WmeUtils.CreateBackupOfFile(enginePath);
        }

        _exportStatus = "reading swz...";

        Dictionary<string, string> dynamicFiles = [];
        foreach (string content in WmeUtils.GetFilesInSwz(dynamicPath, key))
            dynamicFiles.Add(SwzUtils.GetFileName(content), content);
        dynamicFiles[SwzUtils.GetFileName(ldData)] = ldData;

        Dictionary<string, string> initFiles = [];
        foreach (string content in WmeUtils.GetFilesInSwz(initPath, key))
            initFiles.Add(SwzUtils.GetFileName(content), content);
        LevelTypes lts = WmeUtils.DeserializeFromString<LevelTypes>(initFiles["LevelTypes.xml"], bhstyle: true);
        lts.AddOrUpdateLevelType(l.Type ?? throw new ArgumentNullException("l.Type"));
        initFiles["LevelTypes.xml"] = WmeUtils.SerializeToString(lts, minify: true, bhstyle: true);

        Dictionary<string, string> gameFiles = [];
        foreach (string content in WmeUtils.GetFilesInSwz(gamePath, key))
            gameFiles.Add(SwzUtils.GetFileName(content), content);
        LevelSetTypes lst = WmeUtils.DeserializeFromString<LevelSetTypes>(gameFiles["LevelSetTypes.xml"], bhstyle: true);
        UpdatePlaylists(lst, l);
        gameFiles["LevelSetTypes.xml"] = WmeUtils.SerializeToString(lst, minify: true, bhstyle: true);

        _exportStatus = "creating new swz...";
        WmeUtils.SerializeSwzFilesToPath(dynamicPath, dynamicFiles.Values, key);
        WmeUtils.SerializeSwzFilesToPath(initPath, initFiles.Values, key);
        WmeUtils.SerializeSwzFilesToPath(gamePath, gameFiles.Values, key);

        backups.RefreshBackupList(prefs.BrawlhallaPath!);
    }

    private static void UpdatePlaylists(LevelSetTypes levelSetTypes, Level l)
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