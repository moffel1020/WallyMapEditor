using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImGuiNET;
using NativeFileDialogSharp;
using WallyMapEditor.Mod;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public class ModCreatorWindow(PathPreferences prefs)
{
    private readonly record struct LevelFileList(string[] assets, string[] backgrounds, string? thumbnail);
    private readonly record struct ModLevel(Level level, LevelFileList files);

    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    private readonly List<ModLevel> _levels = [];
    // easier to maintain excluded than included
    private readonly HashSet<string> _excludedPaths = [];

    private string? _exportError = null;
    private string? _exportStatus = null;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Mod Creation", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        ImGui.SeparatorText("Brawlhalla path");
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
        ImGui.SeparatorText("Info");
        prefs.ModName = ImGuiExt.InputText("Mod name", prefs.ModName ?? "My mod", 64);
        prefs.ModAuthor = ImGuiExt.InputText("Author", prefs.ModAuthor ?? "", 64);
        prefs.ModVersionInfo = ImGuiExt.InputText("Mod version", prefs.ModVersionInfo ?? "1.0", 16);
        prefs.ModDescription = ImGuiExt.InputTextMultiline("Description", prefs.ModDescription ?? "", new(0, ImGui.GetTextLineHeight() * 8), 1024);
        prefs.GameVersionInfo = ImGuiExt.InputText("Game version", prefs.GameVersionInfo ?? "", 8);

        ImGui.SeparatorText("Maps");
        AddLevelFileButton();

        ImGuiExt.BeginStyledChild("");
        /*foreach (ModLevel ml in _levels)
        {
            Level l = ml.level;
            ImGui.BulletText(l.Desc.LevelName);
        }*/
        foreach (string excluded in _excludedPaths)
            ImGui.BulletText(excluded);
        ImGuiExt.EndStyledChild();

        ImGuiExt.HeaderWithWidget("Files to include", () =>
        {
            ImGuiExt.BeginStyledChild("files");
            foreach (ModLevel ml in _levels)
            {
                Level l = ml.level;
                if (ImGui.TreeNode($"{l.Desc.LevelName}###{l.GetHashCode()}"))
                {
                    LevelFileList files = ml.files;
                    string? thumbnail = files.thumbnail;
                    if (thumbnail is not null)
                        FileCheckbox("Thumbnail: " + thumbnail, MakeGlobal(Path.Combine("images", "thumbnails", thumbnail)));
                    string[] backgrounds = files.backgrounds;
                    if (backgrounds.Length != 0 && ImGui.TreeNode("Backgrounds"))
                    {
                        foreach (string file in backgrounds)
                            FileCheckbox(file, MakeGlobal(Path.Combine("mapArt", "Background", file)));
                        ImGui.TreePop();
                    }
                    string[] assets = files.assets;
                    if (assets.Length != 0 && ImGui.TreeNode($"Assets ({l.Desc.AssetDir})"))
                    {
                        foreach (string file in assets)
                            FileCheckbox(file, MakeGlobal(Path.Combine("mapArt", l.Desc.AssetDir, file)));
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                }
            }
            ImGuiExt.EndStyledChild();
        },
        () =>
        {
            if (ImGui.Button("Refresh"))
            {
                for (int i = 0; i < _levels.Count; ++i)
                {
                    ModLevel ml = _levels[i];
                    LevelFileList files = FindUsedFiles(ml.level);
                    _levels[i] = ml with { files = files };
                }
            }
        }, 60);

        ImGui.Separator();
        ModFileExportButton();

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

    private void FileCheckbox(string text, string path)
    {
        bool fileAdded = !_excludedPaths.Contains(path);
        bool shouldBeAdded = ImGuiExt.Checkbox(text, fileAdded);
        if (shouldBeAdded) _excludedPaths.Remove(path);
        else _excludedPaths.Add(path);
    }

    private void ModFileExportButton()
    {
        if (ImGuiExt.ButtonDisabledIf(!WmeUtils.IsValidBrawlPath(prefs.BrawlhallaPath), "Create mod"))
        {
            ModHeaderObject header = new()
            {
                ModName = prefs.ModName ?? "",
                GameVersionInfo = prefs.GameVersionInfo ?? "",
                ModVersionInfo = prefs.ModVersionInfo ?? "",
                ModDescription = prefs.ModDescription ?? "",
                CreatorInfo = prefs.ModAuthor ?? "",
            };

            Task.Run((Action)(() =>
            {
                try
                {
                    this._exportError = null;
                    _exportStatus = "select file";
                    ModFile mod = CreateModFile(prefs.BrawlhallaPath!, header);
                    DialogResult result = Dialog.FileSave(ModFile.EXTENSION, Path.GetDirectoryName(prefs.ModFilePath));
                    if (result.IsOk)
                    {
                        _exportStatus = "exporting...";
                        string path = WmeUtils.ForcePathExtension(result.Path, '.' + ModFile.EXTENSION);
                        using FileStream stream = new(path, FileMode.Create, FileAccess.Write);
                        mod.Save(stream);
                    }
                }
                catch (Exception e)
                {
                    this._exportError = e.Message;
                }
                finally
                {
                    _exportStatus = null;
                }
            }));
        }
    }

    private void AddLevelFileButton()
    {
        if (ImGui.Button("Add Level file"))
        {
            Task.Run((Action)(() =>
            {
                DialogResult result = Dialog.FileOpenMultiple("xml");
                if (!result.IsOk) return;
                StringBuilder? sb = null;
                foreach (string path in result.Paths)
                {
                    if (!File.Exists(path))
                    {
                        sb ??= new();
                        sb.AppendLine($"Invalid path {path}");
                        continue;
                    }

                    try
                    {
                        XElement element;
                        using (FileStream file = File.OpenRead(path))
                            element = XElement.Load(file);
                        if (element.Name.LocalName != "Level") throw new ArgumentException("Given path does not contain a Level file");
                        Level l = element.DeserializeTo<Level>();
                        _levels.Add(new(l, FindUsedFiles(l)));
                    }
                    catch (Exception e)
                    {
                        sb ??= new();
                        sb.AppendLine($"Error while parsing file: {e.Message}");
                        Rl.TraceLog(Raylib_cs.TraceLogLevel.Error, e.Message);
                        Rl.TraceLog(Raylib_cs.TraceLogLevel.Trace, e.StackTrace);
                    }
                }
                this._exportError = sb?.ToString();
            }));
        }
    }

    private ModFile CreateModFile(string brawlDir, ModHeaderObject header)
    {
        ModFileBuilder builder = new(header);

        HashSet<string> _addedPaths = [];
        foreach (ModLevel ml in _levels)
        {
            Level l = ml.level;
            builder.AddLevel(l);
            LevelFileList files = ml.files;

            bool shouldAddFile(string path) => !_addedPaths.Add(path) && !_excludedPaths.Contains(path);
            foreach (string asset in files.assets)
            {
                if (shouldAddFile(asset))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "mapArt", l.Desc.AssetDir, asset));
            }
            foreach (string bg in files.backgrounds)
            {
                if (shouldAddFile(bg))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "mapArt", "Backgrounds", bg));
            }
            if (files.thumbnail is not null)
            {
                if (shouldAddFile(files.thumbnail))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "images", "thumbnails", files.thumbnail));
            }
        }

        return builder.CreateMod();
    }

    private LevelFileList FindUsedFiles(Level l)
    {
        string? thumbnail = null;
        if (l.Type?.ThumbnailPNGFile is not null)
        {
            thumbnail = NormalizePartialPath("images/thumbnails", l.Type.ThumbnailPNGFile);
        }

        List<string> backgrounds = [];
        foreach (Background bg in l.Desc.Backgrounds)
        {
            backgrounds.Add(NormalizePartialPath("Backgrounds", bg.AssetName));
            if (bg.AnimatedAssetName is not null)
                backgrounds.Add(NormalizePartialPath("Backgrounds", bg.AnimatedAssetName));
        }

        List<string> assets = [];
        foreach (AbstractAsset a in l.Desc.Assets)
        {
            foreach (string path in FindChildAssetNames(a))
                assets.Add(NormalizePartialPath(l.Desc.AssetDir, path));
        }

        return new(assets.ToArray(), backgrounds.ToArray(), thumbnail);
    }

    // removes '../baseDir/' from file paths if they are inside baseDir to make sure they are truly unique
    private static string NormalizePartialPath(string baseDir, string path) =>
        Path.GetRelativePath(baseDir, Path.Combine(baseDir, path)).Replace('\\', '/');

    // use an impossible base path. this is just for the hashset so it's not exposed to the user.
    private static string MakeGlobal(string path) => Path.GetFullPath(path, "Z:/BRAWL/").Replace('\\', '/');

    private static IEnumerable<string> FindChildAssetNames(AbstractAsset a) => a switch
    {
        AbstractAsset asset when asset.AssetName is not null => [asset.AssetName],
        MovingPlatform mp => mp.Assets.SelectMany(FindChildAssetNames),
        Platform p when p.AssetChildren is not null => p.AssetChildren.SelectMany(FindChildAssetNames),
        _ => throw new Exception("Could not find associated assets. Unimplemented AbstractAsset type")
    };
}