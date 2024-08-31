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
    private bool _mapLoadedFromLevel = false;

    private LoadedFile<LevelTypes> _levelTypes = new();
    private LoadedFile<LevelSetTypes> _levelSetTypes = new();

    private LoadedFile<BoneTypes> _boneTypes = new();
    private LoadedFile<string[]> _powerNames = new();

    private string? _pickedFileName;
    private string? _loadingError;
    private string? _loadingStatus;

    private bool _decrypted = false;
    private bool _decrypting = false;

    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    public void Show(LevelLoader loader)
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 490), new(int.MaxValue));
        ImGui.Begin("Import", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        ImGui.BeginTabBar("importTabBar", ImGuiTabBarFlags.None);

        if (ImGui.BeginTabItem("Brawlhalla"))
        {
            ShowGameImportTab(loader);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Files"))
        {
            ShowFileImportTab(loader);
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

    private void ShowGameImportTab(LevelLoader loader)
    {
        ImGui.Text("Import from game swz files");
        ImGui.Separator();
        if (ImGui.Button("Select Brawlhalla Path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                if (result.IsOk)
                    prefs.BrawlhallaPath = result.Path;
            });
        }
        ImGui.Text($"Path: {prefs.BrawlhallaPath}");

        if (WmeUtils.IsValidBrawlPath(prefs.BrawlhallaPath) && ImGuiExt.WithDisabledButton(_decrypting, "Load game files"))
        {
            _decrypted = false;
            _decrypting = true;
            _loadingStatus = "loading...";
            Task.Run(() =>
            {
                try
                {
                    _loadingStatus = "searching key...";
                    WmeUtils.FindDecryptionKeyFromPath(Path.Combine(prefs.BrawlhallaPath!, "BrawlhallaAir.swf"));
                    _loadingStatus = "decrypting...";
                    DecryptSwzFiles(prefs.BrawlhallaPath!);
                    (loader.BoneTypes, loader.PowerNames) = (_boneTypes.Final!, _powerNames.Final!);
                    _decrypted = true;
                    _loadingError = null;
                }
                catch (Exception e)
                {
                    Rl.TraceLog(TraceLogLevel.Error, e.Message);
                    Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
                    _loadingError = $"Could not decrypt swz files. {e.Message}";
                }
                finally
                {
                    _loadingStatus = null;
                    _decrypting = false;
                }
            });
        }

        if (_decrypted && levelDescFiles.Count > 0)
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

            if (ImGuiExt.WithDisabledButton(_pickedFileName is null, "Import LevelDesc"))
            {
                _levelDesc = WmeUtils.DeserializeFromString<LevelDesc>(levelDescFiles[_pickedFileName!], bhstyle: true);
                _levelDescFromSwz = true;
                _levelDescFromPath = false;
                DoLoad(loader);
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

    private void ShowLevelDescImportSection()
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

    private void ShowBoneTypesImportSection(LevelLoader loader)
    {
        ShowFileImportSection(
            "BoneTypes", "xml",
            _boneTypes.FromPath is not null, () => _boneTypes.FromPath = null,
            _boneTypes.Decrypted is not null, () => _boneTypes.Decrypted = null,
            _savedBtPath, path =>
            {
                _savedBtPath = path;
                loader.BoneTypes = _boneTypes.FromPath = WmeUtils.DeserializeFromPath<BoneTypes>(path, bhstyle: true);
            }
        );
    }

    private void ShowPowerNamesImportSection(LevelLoader loader)
    {
        ShowFileImportSection(
            "PowerNames", "csv",
            _powerNames.FromPath is not null, () => _powerNames.FromPath = null,
            _powerNames.Decrypted is not null, () => _powerNames.Decrypted = null,
            _savedPtPath, path =>
            {
                _savedPtPath = path;
                _powerNames.FromPath = WmeUtils.ParsePowerTypesFromPath(path);
                loader.PowerNames = _powerNames.FromPath;
            },
            optional: true
        );
    }

    private void ShowFileImportTab(LevelLoader loader)
    {
        ImGui.PushTextWrapPos();
        ImGui.Text("Import from individual xml and csv files");
        ImGui.Text("When importing from the game these files are loaded from the swz's. You can override them with your own xml or csv files.");
        ImGui.PopTextWrapPos();

        ImGui.Spacing();
        ImGui.SeparatorText("Load map");
        LoadButton(loader);
        ImGui.Spacing();

        ImGui.SeparatorText("Select files");
        ShowLevelDescImportSection();
        ImGui.Separator();
        ShowLevelTypesImportSection();
        ImGui.Separator();
        ShowLevelSetTypesImportSection();
        ImGui.Separator();
        ShowBoneTypesImportSection(loader);
        ImGui.Separator();
        ShowPowerNamesImportSection(loader);
    }

    private void LoadButton(LevelLoader loader)
    {
        if (ImGuiExt.WithDisabledButton(
            _levelDesc is null || _levelTypes.Final is null || _levelSetTypes.Final is null || _boneTypes.Final is null,
            "Load map"
        ))
        {
            DoLoad(loader);
        }
    }

    private void DoLoad(LevelLoader loader)
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
            loader.LoadMapFromData(_levelDesc, _levelTypes.Final, _levelSetTypes.Final, _boneTypes.Final, _powerNames.Final);
        }
        catch (Exception e)
        {
            Rl.TraceLog(TraceLogLevel.Error, e.Message);
            Rl.TraceLog(TraceLogLevel.Trace, e.StackTrace);
            _loadingStatus = null;
            _loadingError = $"Failed to load map file. {e.Message}";
        }
    }

    private void DoLoadFromLevel(LevelLoader loader)
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
            loader.LoadMapFromLevel(_level, _boneTypes.Final, _powerNames.Final);
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

    public void ReImport(LevelLoader loader)
    {
        if (_boneTypes.Final is null)
            return;

        if (_mapLoadedFromLevel)
        {
            if (_level is null)
                return;
            ImportLevelFromPath();
            DoLoadFromLevel(loader);
        }
        else
        {
            if (_levelDesc is null || _levelTypes.Final is null || _levelSetTypes.Final is null)
                return;
            ImportFromPaths();
            DoLoad(loader);
        }
    }

    private void DecryptSwzFiles(string folder)
    {
        string gamePath = Path.Combine(folder, "Game.swz");
        string dynamicPath = Path.Combine(folder, "Dynamic.swz");
        string initPath = Path.Combine(folder, "Init.swz");
        uint key = uint.Parse(prefs.DecryptionKey!);

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