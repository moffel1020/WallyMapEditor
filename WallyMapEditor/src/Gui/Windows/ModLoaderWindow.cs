using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;
using WallyMapEditor.Mod;

namespace WallyMapEditor;

public class ModLoaderWindow(PathPreferences prefs)
{
    private bool _open;
    public bool Open { get => _open; set => _open = value; }

    private readonly HashSet<ModFile> _modFiles = [];
    private bool _backup = true;

    private string? _loadError = null;
    private string? _loadStatus = null;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Mod Loader", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

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
        if (prefs.BrawlhallaPath is null)
            ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
        else
            ImGui.Text($"Selected path: {prefs.BrawlhallaPath}");

        ImGui.SeparatorText("Mod files (.wally)");
        AddModFileButton();
        ShowModFileList();

        ImGui.Separator();

        ImGui.Checkbox("Create backup of swz files", ref _backup);

        if (ImGuiExt.ButtonDisabledIf(_modFiles.Count == 0, "Load mods"))
        {
            Task.Run(() =>
            {
                try
                {
                    _loadError = null;
                    _loadStatus = null;
                    LoadMods();
                }
                catch (Exception e)
                {
                    _loadStatus = null;
                    _loadError = $"Error while loading mods {e.Message}";
                }
            });
        }

        if (_loadError is not null)
            ImGui.TextWrapped($"[Error]: {_loadError}");

        if (_loadStatus is not null)
            ImGui.TextWrapped($"{_loadStatus}");

        ImGui.End();
    }

    private void ShowModFileList()
    {
        ImGuiExt.BeginStyledChild("modfiles");
        List<ModFile> toRemove = [];
        foreach (ModFile mod in _modFiles)
        {
            if (ImGui.Button("x")) toRemove.Add(mod);
            ImGui.SameLine();
            DisplayModInfo(mod);
        }

        foreach (ModFile mod in toRemove)
            _modFiles.Remove(mod);

        ImGuiExt.EndStyledChild();
    }

    private static void DisplayModInfo(ModFile mod)
    {
        ModHeaderObject header = mod.Header;
        if (ImGui.TreeNode($"{header.ModName} - {header.ModVersionInfo}"))
        {
            ImGui.Text($"Author: {header.CreatorInfo}");
            ImGui.Text($"Mod Version: {header.ModVersionInfo}");
            ImGui.Text($"Game Version: {header.GameVersionInfo}");
            ImGui.TextWrapped($"Description:\n{header.ModDescription}");
            if (ImGui.TreeNode($"Maps ({mod.LevelDescs.Length})##{mod.GetHashCode()}"))
            {
                foreach (LevelToPlaylistLinkObject link in mod.LevelToPlaylistLinks)
                    ImGui.Text(link.LevelName);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode($"Extra files ({mod.ExtraFiles.Length})##{mod.GetHashCode()}"))
            {
                foreach (ExtraFileObject extraFile in mod.ExtraFiles)
                    ImGui.Text(extraFile.FullPath);
                ImGui.TreePop();
            }
            ImGui.TreePop();
        }
    }

    private void AddModFileButton()
    {
        if (!ImGui.Button("Add mod file")) return;

        Task.Run(() =>
        {
            DialogResult result = Dialog.FileOpen(ModFile.EXTENSION);
            if (!result.IsOk) return;

            try
            {
                using FileStream stream = new(result.Path, FileMode.Open, FileAccess.Read);
                ModFile mod = ModFile.Load(stream);

                foreach (ModFile other in _modFiles)
                {
                    if (mod.Header.ModName == other.Header.ModName && mod.Header.CreatorInfo == other.Header.CreatorInfo)
                        throw new Exception("Selected mod file is already in the list");
                }

                _modFiles.Add(mod);

                _loadError = null;
            }
            catch (Exception e)
            {
                _loadError = $"Error while loading wally file: {e.Message}";
            }
        });
    }

    private void LoadMods()
    {
        if (_modFiles.Count == 0) return;

        if (!WmeUtils.IsValidBrawlPath(prefs.BrawlhallaPath))
            throw new Exception($"{prefs.BrawlhallaPath} is not a valid brawlhalla path");

        if (_backup)
        {
            _loadStatus = "Creating backups...";
            WmeUtils.CreateBackupOfFile(Path.Combine(prefs.BrawlhallaPath!, "Init.swz"));
            WmeUtils.CreateBackupOfFile(Path.Combine(prefs.BrawlhallaPath!, "Game.swz"));
            WmeUtils.CreateBackupOfFile(Path.Combine(prefs.BrawlhallaPath!, "Engine.swz"));
            WmeUtils.CreateBackupOfFile(Path.Combine(prefs.BrawlhallaPath!, "Dynamic.swz"));
        }

        _loadStatus = "Loading...";
        ModLoader loader = new(prefs.BrawlhallaPath!);
        loader.AddModFiles(_modFiles);
        Dictionary<string, byte[]> files = loader.Load();

        foreach ((string path, byte[] content) in files)
        {
            _loadStatus = $"Writing {path}...";
            string realPath = Path.Combine(prefs.BrawlhallaPath!, path);
            // make sure directory exists
            string? dir = Path.GetDirectoryName(realPath);
            if (dir is not null)
                Directory.CreateDirectory(dir);
            // write out the content
            using FileStream stream = new(realPath, FileMode.Create, FileAccess.Write);
            stream.Write(content);
        }

        _loadStatus = "Done!";
    }
}