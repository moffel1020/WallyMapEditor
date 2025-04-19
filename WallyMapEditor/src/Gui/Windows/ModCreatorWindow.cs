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
    private readonly record struct LevelFileList(string[] Assets, string[] Backgrounds, string? Thumbnail);
    private readonly record struct ModLevel(Level Level, LevelFileList Files);

    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    // when adding new levels in a thread add them to this queue to ensure that the _levels
    // list does not get updated while it is being enumerated
    private readonly Queue<ModLevel> _queuedLevels = [];
    private readonly List<ModLevel> _levels = [];
    // easier to maintain excluded than included
    private readonly HashSet<string> _excludedPaths = [];

    private readonly HashSet<string> _extraFiles = [];

    private string? _exportError = null;
    private string? _exportStatus = null;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Mod Creation", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        if (ImGui.CollapsingHeader("Brawlhalla path"))
        {
            if (ImGui.Button("Select Brawlhalla path"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                    if (result.IsOk)
                        prefs.BrawlhallaPath = result.Path;
                });
            }
            if (prefs.BrawlhallaPath is null)
                ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
            else
                ImGui.Text($"Selected path: {prefs.BrawlhallaPath}");
        }

        ImGui.Spacing();
        ShowModInfoSection();

        ImGui.Spacing();
        if (ImGui.CollapsingHeader("Maps"))
        {
            AddLevelFileButton();
            ShowLevelList();
        }

        ImGui.Spacing();
        ShowExtraFileSection();

        bool showedWarnings = false;
        foreach (ModLevel l in _levels)
        {
            List<string> badStuff = [.. ExportWindow.FindBadMapStuff(l.Level, prefs)];
            if (badStuff.Count > 0)
            {
                if (!showedWarnings)
                {
                    ImGui.SeparatorText("Warnings");
                    showedWarnings = true;
                }

                ImGui.Text($"For {l.Level.Desc.LevelName}");

                foreach (string warning in badStuff)
                    ImGui.TextWrapped("[Warning]: " + warning);
            }
        }

        ImGui.Separator();
        ModFileExportButton();

        if (_exportError is not null)
            ImGui.TextWrapped($"[Error]: {_exportError}");

        if (_exportStatus is not null)
            ImGui.TextWrapped($"{_exportStatus}");

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

            Task.Run(() =>
            {
                try
                {
                    _exportError = null;
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
                    _exportError = e.Message;
                }
                finally
                {
                    _exportStatus = null;
                }
            });
        }
    }

    private void ShowModInfoSection()
    {
        if (!ImGui.CollapsingHeader("Mod Info")) return;
        prefs.ModName = ImGuiExt.InputText("Mod name", prefs.ModName ?? "My mod", 64);
        prefs.ModAuthor = ImGuiExt.InputText("Author", prefs.ModAuthor ?? "", 64);
        prefs.ModVersionInfo = ImGuiExt.InputText("Mod version", prefs.ModVersionInfo ?? "1.0", 16);
        prefs.ModDescription = ImGuiExt.InputTextMultiline("Description", prefs.ModDescription ?? "", new(0, ImGui.GetTextLineHeight() * 8), 1024);
        prefs.GameVersionInfo = ImGuiExt.InputText("Game version", prefs.GameVersionInfo ?? "", 8);
    }

    private void AddLevelFileButton()
    {
        bool res = ImGui.Button("Add level file");
        ImGuiExt.HintTooltip("Level files are obtained by saving with File > Save");
        if (!res) return;

        Task.Run(() =>
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
                    _queuedLevels.Enqueue(new(l, FindUsedFiles(l)));
                }
                catch (Exception e)
                {
                    sb ??= new();
                    sb.AppendLine($"Error while parsing file: {e.Message}");
                    Rl.TraceLog(Raylib_cs.TraceLogLevel.Error, e.Message);
                    Rl.TraceLog(Raylib_cs.TraceLogLevel.Trace, e.StackTrace);
                }
            }
            _exportError = sb?.ToString();
        });
    }

    private void ShowLevelList()
    {
        List<ModLevel> toRemove = [];

        _levels.AddRange(_queuedLevels);
        _queuedLevels.Clear();

        ImGuiExt.BeginStyledChild("files");
        foreach (ModLevel ml in _levels)
        {
            if (ImGui.Button($"x##{ml.GetHashCode()}")) toRemove.Add(ml);
            ImGui.SameLine();

            Level l = ml.Level;
            if (ImGui.TreeNode($"{l.Desc.LevelName}###{l.GetHashCode()}"))
            {
                LevelFileList files = ml.Files;
                string? thumbnail = files.Thumbnail;
                if (thumbnail is not null)
                    FileCheckbox("Thumbnail: " + thumbnail, MakeGlobal(Path.Combine("images", "thumbnails", thumbnail)));
                string[] backgrounds = files.Backgrounds;
                if (backgrounds.Length != 0 && ImGui.TreeNode("Backgrounds"))
                {
                    foreach (string file in backgrounds)
                        FileCheckbox(file, MakeGlobal(Path.Combine("mapArt", "Background", file)));
                    ImGui.TreePop();
                }
                string[] assets = files.Assets;
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

        foreach (ModLevel ml in toRemove)
            _levels.Remove(ml);
    }

    private void ShowExtraFileSection()
    {
        if (!ImGui.CollapsingHeader("Extra Files") || prefs.BrawlhallaPath is null) return;

        if (ImGui.Button("Add file"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(null, prefs.BrawlhallaPath);
                if (result.IsOk)
                    _extraFiles.Add(NormalizePartialPath(prefs.BrawlhallaPath, result.Path));
                // proper validation of the file is done when to mod is created
            });
        }
        ImGuiExt.HintTooltip("Extra files must be inside the brawlhalla directory");

        ImGuiExt.BeginStyledChild("extra files");
        List<string> toRemove = [];
        foreach (string path in _extraFiles)
        {
            if (ImGui.Button($"x##{path}")) toRemove.Add(path);
            ImGui.SameLine();
            ImGui.Text(path);
        }

        foreach (string path in toRemove)
            _extraFiles.Remove(path);

        ImGuiExt.EndStyledChild();
    }

    private ModFile CreateModFile(string brawlDir, ModHeaderObject header)
    {
        ModFileBuilder builder = new(header);

        HashSet<string> _addedPaths = [];
        foreach (ModLevel ml in _levels)
        {
            Level l = ml.Level;
            builder.AddLevel(l);
            LevelFileList files = ml.Files;

            bool shouldAddFile(string path) => _addedPaths.Add(path) && !_excludedPaths.Contains(path);
            foreach (string asset in files.Assets)
            {
                if (shouldAddFile(asset))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "mapArt", l.Desc.AssetDir, asset));
            }
            foreach (string bg in files.Backgrounds)
            {
                if (shouldAddFile(bg))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "mapArt", "Backgrounds", bg));
            }
            if (files.Thumbnail is not null)
            {
                if (shouldAddFile(files.Thumbnail))
                    builder.AddFilePath(brawlDir, Path.Combine(brawlDir, "images", "thumbnails", files.Thumbnail));
            }
        }

        foreach (string path in _extraFiles)
        {
            // excludedPaths keeps track of excluded level assets, don't need to check it here
            if (_addedPaths.Add(path))
                builder.AddFilePath(brawlDir, Path.Combine(brawlDir, path));
        }

        return builder.CreateMod();
    }

    private static LevelFileList FindUsedFiles(Level l)
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

        return new([.. assets], [.. backgrounds], thumbnail);
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