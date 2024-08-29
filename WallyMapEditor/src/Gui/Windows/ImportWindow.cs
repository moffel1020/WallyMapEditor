using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using WallyMapSpinzor2;
using BrawlhallaSwz;

using Raylib_cs;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public class ImportWindow(PathPreferences prefs)
{
    public const int MAX_KEY_LENGTH = 9;

    private string? _savedLdPath = prefs.LevelDescPath;
    private string? _savedLtPath = prefs.LevelTypePath;
    private string? _savedLstPath = prefs.LevelSetTypesPath;
    private string? _savedLPath = prefs.LevelPath;
    private string? _savedBtPath = prefs.BoneTypesPath;
    private string? _savedPtPath = prefs.PowerTypesPath;

    private readonly Dictionary<string, string> levelDescFiles = [];
    private string _levelDescFileFilter = "";

    private struct LoadedFile<T>
    {
        public T? FromPath;
        public T? Decrypted;

        public readonly T? Final => FromPath ?? Decrypted;
    }

    private LevelDesc? _levelDesc = null;
    private bool? _levelDescFromSwz = null;
    private bool? _levelDescFromPath = null;

    private Level? _level = null;
    private bool _selectedLevelFile = false;
    private bool _mapLoadedFromLevel = false;

    private LoadedFile<LevelTypes> _levelTypes = new();
    private LoadedFile<LevelSetTypes> _levelSetTypes = new();

    private LoadedFile<BoneTypes> _boneTypes = new();
    private LoadedFile<string[]> _powerNames = new();

    private string? _pickedFileName;

    private string? _loadingError;
    private string? _loadingStatus;

    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    public void Show(Editor editor)
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 410), new(int.MaxValue));
        ImGui.Begin("Import", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        ImGui.BeginTabBar("importTabBar", ImGuiTabBarFlags.None);

        if (ImGui.BeginTabItem("Files"))
        {
            ShowFileImportTab(editor);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Quick load"))
        {
            ShowLevelImportTab(editor);
            ImGui.EndTabItem();
        }

        if (_loadingStatus is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text(_loadingStatus);
            ImGui.PopTextWrapPos();
        }

        if (_loadingError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("[Error]: " + _loadingError);
            ImGui.PopTextWrapPos();
        }

        ImGui.EndTabBar();

        ImGui.End();
    }

    private void ShowLevelImportTab(Editor editor)
    {
        ImGui.PushTextWrapPos();
        ImGui.Text("Load maps saved with the quick export option.");
        ImGui.Text("There are mandatory files that are not included in this format. So you will need to load them too");
        ImGui.PopTextWrapPos();
        LevelLoadButton(editor);

        if (ImGui.Button("Select file"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(_savedLPath));
                if (result.IsOk)
                {
                    _loadingStatus = "loading...";
                    try
                    {
                        _selectedLevelFile = true;
                        _savedLPath = result.Path;
                        _level = WmeUtils.DeserializeFromPath<Level>(result.Path, bhstyle: true);
                        _loadingStatus = null;
                        _loadingError = null;
                    }
                    catch (Exception e)
                    {
                        _loadingStatus = null;
                        _loadingError = $"failed to load file. {e.Message}";
                        Rl.TraceLog(TraceLogLevel.Error, e.Message);
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    }
                }
            });
        }

        if (_selectedLevelFile)
        {
            ImGui.SameLine();
            ImGui.Text(_savedLPath ?? "");
        }
    }

    private void ShowSwzDecryptSection()
    {
        if (ImGui.Button($"Select Brawlhalla Path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                if (result.IsOk && WmeUtils.IsValidBrawlPath(result.Path))
                    prefs.BrawlhallaPath = result.Path;
            });
        }
        ImGui.Text($"Path: {prefs.BrawlhallaPath}");
        if (!string.IsNullOrWhiteSpace(prefs.BrawlhallaPath))
        {
            if (ImGui.Button("Decrypt"))
            {
                Task.Run(() =>
                {
                    try
                    {
                        _loadingStatus = "finding key...";
                        uint key = WmeUtils.FindDecryptionKeyFromPath(Path.Combine(prefs.BrawlhallaPath, "BrawlhallaAir.swf")) ?? throw new Exception("could not find decryption key");
                        _loadingStatus = "decrypting...";
                        DecryptSwzFiles(prefs.BrawlhallaPath, key);
                        prefs.DecryptionKey = key.ToString();
                        _loadingStatus = null;
                        _loadingError = null;
                    }
                    catch (Exception e)
                    {
                        _loadingError = e.Message;
                    }
                });
            }
        }
    }

    private void ShowFileImportSection(
        string sectionName, string fileExt,
        bool hasFromPath, Action removeFromPath,
        bool hasDecrypted, Action removeDecrypted,
        string? path, Action<string> loadFromPath,
        bool optional = false
    )
    {
        if (hasFromPath)
        {
            if (ImGui.Button($"x##{sectionName}_frompath")) removeFromPath();
            ImGui.SameLine();
            ImGui.Text($"{sectionName} is loaded from a file");
        }
        else if (hasDecrypted)
        {
            if (ImGui.Button($"x##{sectionName}_swz")) removeDecrypted();
            ImGui.SameLine();
            ImGui.Text($"{sectionName} is loaded from swz");
        }
        else
            ImGui.Text(optional ? $"{sectionName} is optional" : $"{sectionName} needs to be loaded");

        if (path is not null)
        {
            if (ImGui.Button($"import from saved path##{sectionName}"))
            {
                _loadingStatus = "loading...";
                Task.Run(() =>
                {
                    try
                    {
                        loadFromPath(path);
                        _loadingStatus = null;
                        _loadingError = null;
                    }
                    catch (Exception e)
                    {
                        _loadingStatus = null;
                        _loadingError = $"failed to load file. {e.Message}";
                        Rl.TraceLog(TraceLogLevel.Error, e.Message);
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    }
                });
            }
            ImGui.SameLine();
            ImGui.Text(path);
        }
        if (ImGui.Button($"Select file##{sectionName}"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(fileExt, Path.GetDirectoryName(path));
                if (result.IsOk)
                {
                    _loadingStatus = "loading...";
                    try
                    {
                        loadFromPath(result.Path);
                        _loadingStatus = null;
                        _loadingError = null;
                    }
                    catch (Exception e)
                    {
                        _loadingStatus = null;
                        _loadingError = $"failed to load file. {e.Message}";
                        Rl.TraceLog(TraceLogLevel.Error, e.Message);
                        Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    }
                }
            });
        }
    }

    private void ShowLevelDescImportSection(Editor editor)
    {
        ShowFileImportSection(
            "LevelDesc", "xml",
            _levelDescFromPath == true, () => { _levelDescFromPath = _levelDescFromSwz = null; _levelDesc = null; },
            _levelDescFromSwz == true, () => { _levelDescFromPath = _levelDescFromSwz = null; _levelDesc = null; },
            _savedLdPath, path =>
            {
                _savedLdPath = path;
                _levelDesc = WmeUtils.DeserializeFromPath<LevelDesc>(path, bhstyle: true);
                _levelDescFromSwz = false;
                _levelDescFromPath = true;
            }
        );
        ImGui.SameLine();
        ImGui.Text("or");
        ImGui.SameLine();
        if (ImGui.Button("Pick from game")) ImGui.OpenPopup("PickFromGame");
        if (ImGui.BeginPopup("PickFromGame") && levelDescFiles.Count > 0)
        {
            _levelDescFileFilter = ImGuiExt.InputText("Filter map names", _levelDescFileFilter);
            string[] levelDescs = levelDescFiles.Keys
                .Where(s => s.Contains(_levelDescFileFilter, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            int pickedItem = Array.FindIndex(levelDescs, s => s == _pickedFileName);
            if (ImGui.ListBox("Pick level file from swz", ref pickedItem, levelDescs, levelDescs.Length, 12))
            {
                _pickedFileName = levelDescs[pickedItem];
            }

            if (ImGuiExt.WithDisabledButton(_pickedFileName is null, "Load map"))
            {
                _levelDesc = WmeUtils.DeserializeFromString<LevelDesc>(levelDescFiles[_pickedFileName!], bhstyle: true);
                _levelDescFromSwz = true;
                _levelDescFromPath = false;
                DoLoad(editor);
            }
            ImGui.EndPopup();
        }
    }

    private void ShowLevelTypesImportSection()
    {
        ShowFileImportSection(
            "LevelTypes", "xml",
            _levelTypes.FromPath is not null, () => _levelTypes.FromPath = null,
            _levelTypes.Decrypted is not null, () => _levelTypes.Decrypted = null,
            _savedLtPath, path =>
            {
                _savedLtPath = path;
                _levelTypes.FromPath = WmeUtils.DeserializeFromPath<LevelTypes>(path, bhstyle: true);
            }
        );
    }

    private void ShowLevelSetTypesImportSection()
    {
        ShowFileImportSection(
            "LevelSetTypes", "xml",
            _levelSetTypes.FromPath is not null, () => _levelSetTypes.FromPath = null,
            _levelSetTypes.Decrypted is not null, () => _levelSetTypes.Decrypted = null,
            _savedLstPath, path =>
            {
                _savedLstPath = path;
                _levelSetTypes.FromPath = WmeUtils.DeserializeFromPath<LevelSetTypes>(path, bhstyle: true);
            }
        );
    }

    private void ShowBoneTypesImportSection()
    {
        ShowFileImportSection(
            "BoneTypes", "xml",
            _boneTypes.FromPath is not null, () => _boneTypes.FromPath = null,
            _boneTypes.Decrypted is not null, () => _boneTypes.Decrypted = null,
            _savedBtPath, path =>
            {
                _savedBtPath = path;
                _boneTypes.FromPath = WmeUtils.DeserializeFromPath<BoneTypes>(path, bhstyle: true);
            }
        );
    }

    private void ShowPowerNamesImportSection()
    {
        ShowFileImportSection(
            "PowerNames", "csv",
            _powerNames.FromPath is not null, () => _powerNames.FromPath = null,
            _powerNames.Decrypted is not null, () => _powerNames.Decrypted = null,
            _savedPtPath, path =>
            {
                _savedPtPath = path;
                _powerNames.FromPath = WmeUtils.ParsePowerTypesFromPath(path);
            },
            optional: true
        );
    }

    private void ShowFileImportTab(Editor editor)
    {
        ImGui.Text("Import from game or individual files");
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("When importing from the game these files are loaded from the swz's.\nYou can override them with your own xml or csv files.");

        if (_levelTypes.Final is null || _levelSetTypes.Final is null || _boneTypes.Final is null)
        {
            ImGui.SeparatorText("Load from game"); 
            ShowSwzDecryptSection();
        }
        else
        {
            ImGui.Separator();
            ShowLevelDescImportSection(editor);
            ImGui.Spacing();
            ImGui.SeparatorText("Load map");
            LoadButton(editor);
            ImGui.Spacing();
        }

        ImGui.Separator();
        if (ImGui.CollapsingHeader("Override individual files"))
        {
            ShowLevelTypesImportSection();
            ImGui.Separator();
            ShowLevelSetTypesImportSection();
            ImGui.Separator();
            ShowBoneTypesImportSection();
            ImGui.Separator();
            ShowPowerNamesImportSection();
        }
    }

    private void LoadButton(Editor editor)
    {
        if (ImGuiExt.WithDisabledButton(
            _levelDesc is null || _levelTypes.Final is null || _levelSetTypes.Final is null || _boneTypes.Final is null,
            "Load map"
        ))
        {
            DoLoad(editor);
        }
    }

    private void LevelLoadButton(Editor editor)
    {
        if (ImGuiExt.WithDisabledButton(
            _level is null || _boneTypes.Final is null,
            "Load map"
        ))
        {
            DoLoadFromLevel(editor);
        }
    }

    private void DoLoad(Editor editor)
    {
        if (_levelDesc is null || _levelTypes.Final is null || _levelSetTypes.Final is null || _boneTypes.Final is null)
            return;

        _loadingStatus = "loading...";
        try
        {
            // scuffed xml parse error handling
            if (_levelDesc.CameraBounds is null) throw new System.Xml.XmlException("LevelDesc xml did not contain essential elements");

            _loadingStatus = null;
            _loadingError = null;
            _mapLoadedFromLevel = false;
            editor.LoadMapFromLevel(new Level(_levelDesc, _levelTypes.Final, _levelSetTypes.Final), _boneTypes.Final, _powerNames.Final);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, e.Message);
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
            _loadingStatus = null;
            _loadingError = $"Failed to load map file. {e.Message}";
        }
    }

    private void DoLoadFromLevel(Editor editor)
    {
        if (_level is null || _boneTypes.Final is null)
            return;

        _loadingStatus = "loading...";
        try
        {
            // scuffed xml parse error handling
            if (_level.Desc is null) throw new System.Xml.XmlException("Level xml did not contain essential elements");
            _loadingStatus = null;
            _loadingError = null;
            _mapLoadedFromLevel = true;
            editor.LoadMapFromLevel(_level, _boneTypes.Final, _powerNames.Final);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, e.Message);
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
            _loadingStatus = null;
            _loadingError = $"Failed to load map file. {e.Message}";
        }
    }

    private void ImportFromPaths()
    {
        if (_levelDesc is not null && _levelDescFromPath == true && _savedLdPath is not null)
        {
            _levelDesc = WmeUtils.DeserializeFromPath<LevelDesc>(_savedLdPath, bhstyle: true);
            _levelDescFromPath = true;
            _levelDescFromSwz = false;
        }

        if (_levelTypes.FromPath is not null && _savedLtPath is not null)
            _levelTypes.FromPath = WmeUtils.DeserializeFromPath<LevelTypes>(_savedLtPath, bhstyle: true);
        if (_levelSetTypes.FromPath is not null && _savedLstPath is not null)
            _levelSetTypes.FromPath = WmeUtils.DeserializeFromPath<LevelSetTypes>(_savedLstPath, bhstyle: true);
        if (_boneTypes.FromPath is not null && _savedBtPath is not null)
            _boneTypes.FromPath = WmeUtils.DeserializeFromPath<BoneTypes>(_savedBtPath, bhstyle: true);
        if (_powerNames.FromPath is not null && _savedPtPath is not null)
            _powerNames.FromPath = WmeUtils.ParsePowerTypesFromPath(_savedPtPath);
    }

    private void ImportLevelFromPath()
    {
        if (_level is not null && _savedLPath is not null)
            _level = WmeUtils.DeserializeFromPath<Level>(_savedLPath, bhstyle: true);
    }

    public bool CanReImport() =>
    (
        _level is not null ||
        (
            _levelDesc is not null &&
            _levelTypes.Final is not null &&
            _levelSetTypes.Final is not null
        )
    ) &&
    _boneTypes.Final is not null;

    public void ReImport(Editor editor)
    {
        if (_boneTypes.Final is null)
            return;

        if (_mapLoadedFromLevel)
        {
            if (_level is null)
                return;
            ImportLevelFromPath();
            DoLoadFromLevel(editor);
        }
        else
        {
            if (_levelDesc is null || _levelTypes.Final is null || _levelSetTypes.Final is null)
                return;
            ImportFromPaths();
            DoLoad(editor);
        }
    }

    private void DecryptSwzFiles(string folder, uint key)
    {
        string gamePath = Path.Combine(folder, "Game.swz");
        string dynamicPath = Path.Combine(folder, "Dynamic.swz");
        string initPath = Path.Combine(folder, "Init.swz");

        levelDescFiles.Clear();
        foreach (string file in WmeUtils.GetFilesInSwz(dynamicPath, key))
        {
            string name = SwzUtils.GetFileName(file);
            if (!name.StartsWith("LevelDesc_"))
                continue;
            levelDescFiles.Add(name["LevelDesc_".Length..], file);
        }

        _levelTypes.Decrypted = WmeUtils.DeserializeSwzFromPath<LevelTypes>(initPath, "LevelTypes.xml", key, bhstyle: true);
        _levelSetTypes.Decrypted = WmeUtils.DeserializeSwzFromPath<LevelSetTypes>(gamePath, "LevelSetTypes.xml", key, bhstyle: true);
        _boneTypes.Decrypted = WmeUtils.DeserializeSwzFromPath<BoneTypes>(initPath, "BoneTypes.xml", key, bhstyle: true);

        string? powerTypesContent = WmeUtils.GetFileInSwzFromPath(gamePath, "powerTypes.csv", key);
        _powerNames.Decrypted = powerTypesContent is null
            ? null
            : WmeUtils.ParsePowerTypesFromString(powerTypesContent);
    }
}