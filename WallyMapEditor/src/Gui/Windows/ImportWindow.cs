using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

using BrawlhallaSwz;
using WallyMapEditor.Mod;

using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public class ImportWindow
{
    public const int MAX_KEY_LENGTH = 9;

    private string? _swzDescName;
    private string? _savedLdPath;
    private string? _savedLtPath;
    private string? _savedLstPath;
    private string? _savedBtPath;
    private string? _savedPtPath;
    private string? _savedModPath;
    private string _keyInput = "";

    private bool _searchingDescNames = false;
    private bool _shouldCloseDescPopup = false;
    private string[] levelDescNames = [];
    private string _levelDescFileFilter = "";

    private string? _loadingError;
    private string? _loadingStatus;

    private ModFileLoad? _modFileLoad;

    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    public PathPreferences Prefs { get; set; }

    public ImportWindow(PathPreferences prefs)
    {
        Prefs = prefs;
        _keyInput = prefs.DecryptionKey ?? "";
        Prefs.BrawlhallaPathChanged += (_, path) => UpdateModFileBrawlPath(path);
    }

    public void Show(LevelLoader loader)
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 400), new(int.MaxValue));
        ImGui.Begin("Import", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        ImGui.BeginTabBar("importTabBar", ImGuiTabBarFlags.None);
        if (ImGui.BeginTabItem("Level"))
        {
            ShowImportMenu(loader);
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem($"Mod file (.{ModFile.EXTENSION})"))
        {
            ShowModFileImportMenu(loader);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        if (_loadingStatus is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("[Status]: " + _loadingStatus);
            ImGui.PopTextWrapPos();
        }

        if (_loadingError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("[Error]: " + _loadingError);
            ImGui.PopTextWrapPos();
        }

        ImGui.End();
    }

    private const string MODFILE_IMPORT_WARNING = @"Note: importing mods files into the editor is advanced usage. 
If you just want to play with mods in game, use the menu under Mods > Load mods";

    private void ShowModFileImportMenu(LevelLoader loader)
    {
        ImGui.TextWrapped(MODFILE_IMPORT_WARNING);
        ImGui.Separator();

        if (loader.BoneTypes is null)
        {
            ImGui.Text("Required files are missing.\nPress \"Load required files only\" in the level import tab first");
            return;
        }

        ImGui.Text("Import from mod file. \nThis may add extra files like images to your brawlhalla directory.");

        ImGui.SeparatorText("Brawlhalla path");
        if (ImGui.Button("Select Brawlhalla Path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(Prefs.BrawlhallaPath);
                if (result.IsOk)
                    Prefs.BrawlhallaPath = result.Path;
            });
        }
        if (Prefs.BrawlhallaPath is null)
            ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
        else
            ImGui.Text($"Selected path: {Prefs.BrawlhallaPath}");
        ImGui.SeparatorText("Mod file");

        if (ImGuiExt.ButtonDisabledIf(!WmeUtils.IsValidBrawlPath(Prefs.BrawlhallaPath), "Select mod file"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(ModFile.EXTENSION, Path.GetDirectoryName(Prefs.ModFilePath));
                if (result.IsOk)
                {
                    _savedModPath = result.Path;
                    CreateModFileLoad(_savedModPath, Prefs);
                }
            });
        }
        if (Prefs.ModFilePath is not null)
        {
            ImGui.SameLine();
            ImGui.Text("or");
            ImGui.SameLine();
            if (ImGui.Button("Use last path"))
            {
                _savedModPath = Prefs.ModFilePath;
                CreateModFileLoad(_savedModPath, Prefs);
            }
            ImGui.SameLine();
            ImGui.Text(Prefs.ModFilePath);
        }
        ImGui.Text($"Path: {_savedModPath}");

        if (_modFileLoad is not null && _modFileLoad.ModFile is not null)
        {
            ImGui.Separator();
            if (ImGui.CollapsingHeader("Mod info##header"))
            {
                ImGuiExt.BeginStyledChild("##ModInfoWindow");
                ModHeaderObject header = _modFileLoad.ModFile.Header;
                ImGui.Text($"Name: {header.ModName}");
                ImGui.Text($"Author: {header.CreatorInfo}");
                ImGui.Text($"Mod Version: {header.ModVersionInfo}");
                ImGui.Text($"Description:\n{header.ModDescription}");
                ImGui.Text($"Game Version: {header.GameVersionInfo}");
                ImGui.SeparatorText("Extra files");
                foreach (ExtraFileObject extraFile in _modFileLoad.ModFile.ExtraFiles)
                    ImGui.Text(extraFile.FullPath);
                ImGuiExt.EndStyledChild();
            }
        }
        ImGui.Separator();
        if (ImGuiExt.ButtonDisabledIf(_modFileLoad is null, "Import"))
            Task.Run(() =>
            {
                try
                {
                    _loadingError = null;
                    _loadingStatus = "loading...";
                    loader.LoadMap(_modFileLoad!);
                }
                catch (Exception e)
                {
                    _loadingError = e.Message;
                }
                finally
                {
                    _loadingStatus = null;
                }
            });
    }

    private void UpdateModFileBrawlPath(string brawlPath)
    {
        if (_modFileLoad is not null && _modFileLoad.BrawlPath != brawlPath)
            _modFileLoad = new ModFileLoad(_modFileLoad.FilePath, brawlPath);
    }

    private void CreateModFileLoad(string path, PathPreferences prefs)
    {
        if (_modFileLoad is null || path != _modFileLoad.FilePath || prefs.BrawlhallaPath! != _modFileLoad.BrawlPath)
            _modFileLoad = new ModFileLoad(path, prefs.BrawlhallaPath!);

        prefs.ModFilePath = path;
        _modFileLoad?.CacheModFile();
    }

    private static void ShowFileImportSection(
        string sectionName, string fileExt,
        string? path, Action<string?> setPath,
        string? prefsPath,
        bool canLoadFromSwz = true
    )
    {
        if (path is not null)
        {
            if (ImGui.Button($"x##{sectionName}_frompath")) setPath(null);
            ImGui.SameLine();
            ImGui.Text($"{sectionName} is loaded from a file");
            ImGui.Text(path);
        }
        else
        {
            ImGui.Text(canLoadFromSwz ? $"{sectionName} will be loaded from swz" : $"{sectionName} needs to be selected");
        }

        if (prefsPath is not null)
        {
            if (ImGui.Button($"import from saved path##{sectionName}"))
                setPath(prefsPath);
            ImGui.SameLine();
            ImGui.Text(prefsPath);
        }

        if (ImGui.Button($"Select file##{sectionName}"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(fileExt, Path.GetDirectoryName(path));
                if (result.IsOk)
                    setPath(result.Path);
            });
        }
    }

    private void ShowLevelDescImportSection()
    {
        ShowFileImportSection(
            "LevelDesc", "xml",
            _savedLdPath, path => _savedLdPath = path,
            Prefs.LevelDescPath,
            canLoadFromSwz: false
        );
        ImGui.SameLine();
        ImGui.Text("or");
        ImGui.SameLine();
        if (ImGui.Button("Pick from game"))
        {
            _shouldCloseDescPopup = false;
            ImGui.OpenPopup("SelectGameLd");
        }

        if (ImGui.BeginPopup("SelectGameLd"))
        {
            if (_shouldCloseDescPopup) // can't close in thread so have to do this
            {
                _shouldCloseDescPopup = false;
                ImGui.CloseCurrentPopup();
            }

            if (!_searchingDescNames && Prefs.BrawlhallaPath is not null && levelDescNames.Length == 0)
            {
                _searchingDescNames = true;
                Task.Run(() =>
                {
                    try
                    {
                        _loadingError = null;
                        uint key = uint.Parse(_keyInput);
                        levelDescNames = FindLevelDescNames(Prefs.BrawlhallaPath, key);
                        Prefs.DecryptionKey = _keyInput;
                        _searchingDescNames = false;
                    }
                    catch (Exception e)
                    {
                        _loadingError = e.Message;
                        _searchingDescNames = false;
                        _shouldCloseDescPopup = true;
                    }
                });
            }

            if (levelDescNames.Length > 0)
            {
                _levelDescFileFilter = ImGuiExt.InputText("Filter map names", _levelDescFileFilter);
                string[] levelDescs = levelDescNames
                    .Where(s => s.Contains(_levelDescFileFilter, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                int pickedItem = Array.FindIndex(levelDescs, s => s == _swzDescName);
                if (ImGui.ListBox("Pick level file from swz", ref pickedItem, levelDescs, levelDescs.Length, 12))
                    _swzDescName = levelDescs[pickedItem];

                if (ImGui.Button("Select")) ImGui.CloseCurrentPopup();
            }
            else
            {
                ImGui.Text("Loading...");
            }

            ImGui.EndPopup();
        }

        if (_savedLdPath is null && _swzDescName is not null) ImGui.Text($"From game: {_swzDescName}");
    }

    private void ShowLevelTypesImportSection()
    {
        ShowFileImportSection(
            "LevelTypes", "xml",
            _savedLtPath, path => _savedLtPath = path,
            Prefs.LevelTypesPath
        );
    }

    private void ShowLevelSetTypesImportSection()
    {
        ShowFileImportSection(
            "LevelSetTypes", "xml",
            _savedLstPath, path => _savedLstPath = path,
            Prefs.LevelSetTypesPath
        );
    }

    private void ShowBoneTypesImportSection()
    {
        ShowFileImportSection(
            "BoneTypes", "xml",
            _savedBtPath, path => _savedBtPath = path,
            Prefs.BoneTypesPath
        );
    }

    private void ShowPowerNamesImportSection()
    {
        ShowFileImportSection(
            "PowerNames", "csv",
            _savedPtPath, path => _savedPtPath = path,
            Prefs.PowerTypesPath
        );
    }

    private void ShowImportMenu(LevelLoader loader)
    {
        ImGui.Text("Import from game");
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("When importing from the game these files are loaded from the swz's.\nYou can override them with your own xml or csv files.");

        ImGui.SeparatorText("Brawlhalla path");
        if (ImGui.Button("Select Brawlhalla Path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(Prefs.BrawlhallaPath);
                if (result.IsOk)
                    Prefs.BrawlhallaPath = result.Path;
            });
        }
        if (Prefs.BrawlhallaPath is null)
            ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
        else
            ImGui.Text($"Selected path: {Prefs.BrawlhallaPath}");
        ImGui.SeparatorText("Decryption key");

        ImGui.InputText("Decryption key", ref _keyInput, 9, ImGuiInputTextFlags.CharsDecimal);
        if (Prefs.BrawlhallaPath is not null && ImGui.Button("Find key"))
        {
            Task.Run(() =>
            {
                try
                {
                    _loadingError = null;
                    _loadingStatus = "searching key...";
                    _keyInput = WmeUtils.FindDecryptionKeyFromPath(Path.Combine(Prefs.BrawlhallaPath, "BrawlhallaAir.swf"))?.ToString() ?? "";
                }
                catch (Exception e)
                {
                    _loadingError = e.Message;
                }
                finally
                {
                    _loadingStatus = null;
                }
            });
        }

        ImGui.Spacing();
        ImGui.SeparatorText("Select files");
        ShowLevelDescImportSection();
        ImGui.Spacing();

        ImGui.SeparatorText("Load");
        LoadButton(loader);
        RequiredFilesLoadButton(loader);
        ImGui.Spacing();

        ImGui.Separator();
        if (ImGui.CollapsingHeader("Override paths"))
        {
            ImGui.Separator();
            ShowLevelTypesImportSection();
            ImGui.Separator();
            ShowLevelSetTypesImportSection();
            ImGui.Separator();
            ShowBoneTypesImportSection();
            ImGui.Separator();
            ShowPowerNamesImportSection();
        }

#if DEBUG
        // secret stress testing option :3
        if (Prefs.BrawlhallaPath is not null && uint.TryParse(_keyInput, out uint decryptionKey))
        {
            ImGui.Separator();
            if (ImGui.Button("stress test"))
            {
                _loadingError = LoadStressTester.StressTest(Prefs.BrawlhallaPath, decryptionKey);
            }
        }
#endif
    }

    private void LoadButton(LevelLoader loader)
    {
        if (ImGuiExt.ButtonDisabledIf(!uint.TryParse(_keyInput, out uint decryptionKey)
            && !WmeUtils.IsValidBrawlPath(Prefs.BrawlhallaPath) || (_savedLdPath is null && _swzDescName is null), "Load map"))
        {
            Task.Run(() =>
            {
                try
                {
                    ILoadMethod loadMethod = new OverridableGameLoad
                    (
                        brawlPath: Prefs.BrawlhallaPath!,
                        swzLevelName: _savedLdPath is null ? _swzDescName : null,
                        key: decryptionKey,
                        descPath: _savedLdPath,
                        typesPath: _savedLtPath,
                        setTypesPath: _savedLstPath,
                        bonesPath: _savedBtPath,
                        powersPath: _savedPtPath
                    );
                    _loadingStatus = "loading...";
                    _loadingError = null;
                    loader.LoadMap(loadMethod);

                    Prefs.DecryptionKey = _keyInput;
                    Prefs.LevelDescPath = _savedLdPath ?? Prefs.LevelDescPath;
                    Prefs.LevelTypesPath = _savedLtPath ?? Prefs.LevelTypesPath;
                    Prefs.LevelSetTypesPath = _savedLstPath ?? Prefs.LevelSetTypesPath;
                    Prefs.BoneTypesPath = _savedBtPath ?? Prefs.BoneTypesPath;
                    Prefs.PowerTypesPath = _savedPtPath ?? Prefs.PowerTypesPath;
                }
                catch (Exception e)
                {
                    _loadingError = e.Message;
                }
                finally
                {
                    _loadingStatus = null;
                }
            });
        }
    }

    private void RequiredFilesLoadButton(LevelLoader loader)
    {
        if (ImGuiExt.ButtonDisabledIf(Prefs.BrawlhallaPath is null, "Load required files only"))
        {
            Task.Run(() =>
            {
                try
                {
                    _loadingStatus = "loading...";
                    _loadingError = null;
                    LoadRequiredFilesOnly(loader, Prefs.BrawlhallaPath!);
                }
                catch (Exception e)
                {
                    _loadingError = e.Message;
                }
                finally
                {
                    _loadingStatus = null;
                }
            });
        }
    }

    private void LoadRequiredFilesOnly(LevelLoader loader, string brawlPath)
    {
        string initPath = Path.Combine(brawlPath, "Init.swz");
        string gamePath = Path.Combine(brawlPath, "Game.swz");

        uint key = _savedBtPath is null || _savedPtPath is null
            ? WmeUtils.FindDecryptionKeyFromPath(Path.Combine(brawlPath, "BrawlhallaAir.swf")) ?? throw new Exception("Could not find decryption key")
            : 0;

        loader.BoneTypes = _savedBtPath is null
            ? WmeUtils.DeserializeSwzFromPath<BoneTypes>(initPath, "BoneTypes.xml", key, bhstyle: true) ?? throw new FileLoadException("Could not load BoneTypes from swz")
            : WmeUtils.DeserializeFromPath<BoneTypes>(_savedBtPath, bhstyle: true);

        string? powerTypesContent = WmeUtils.GetFileInSwzFromPath(gamePath, "powerTypes.csv", key);
        loader.PowerNames = powerTypesContent is null
            ? null
            : WmeUtils.ParsePowerTypesFromString(powerTypesContent);

    }

    private static string[] FindLevelDescNames(string brawlPath, uint key)
    {
        string dynamicPath = Path.Combine(brawlPath, "Dynamic.swz");

        return WmeUtils.GetFilesInSwz(dynamicPath, key)
            .Select(SwzUtils.GetFileName)
            .Where(n => n.StartsWith("LevelDesc_"))
            .Select(n => n["LevelDesc_".Length..])
            .ToArray();
    }
}