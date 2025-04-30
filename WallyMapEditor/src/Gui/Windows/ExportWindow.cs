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
        if (prefs.BrawlhallaPath is null)
            ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
        else
            ImGui.Text($"Selected path: {prefs.BrawlhallaPath}");

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

        List<string> mapWarnings = [.. ValidateMapForGame(l, prefs)];
        if (mapWarnings.Count > 0)
        {
            ImGui.SeparatorText("Warnings");
            foreach (string warning in mapWarnings)
                ImGui.TextWrapped("[Warning]: " + warning);
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

        string dynamicPath = Path.Combine(prefs.BrawlhallaPath, "Dynamic.swz");
        string initPath = Path.Combine(prefs.BrawlhallaPath, "Init.swz");
        string gamePath = Path.Combine(prefs.BrawlhallaPath, "Game.swz");
        string enginePath = Path.Combine(prefs.BrawlhallaPath, "Engine.swz");

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

        backups.RefreshBackupList(prefs.BrawlhallaPath);
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

    public static IEnumerable<string> ValidateMapForGame(Level l, PathPreferences prefs)
    {
        LevelType? lt = l.Type;
        if (lt is not null && lt.LevelID > LevelTypes.MAX_LEVEL_ID)
            yield return $"LevelType has LevelID {lt.LevelID}, which is greater than {LevelTypes.MAX_LEVEL_ID}.";

        LevelDesc ld = l.Desc;
        if (ld.NavNodes.Length == 0 && ld.DynamicNavNodes.All((dn) => dn.Children.Length == 0))
            yield return "The LevelDesc has no NavNodes. This will cause a crash when using a bot.";
        if (ld.Respawns.Length == 0 && ld.DynamicRespawns.All((dr) => dr.Children.Length == 0))
            yield return "The LevelDesc has no Respawns. This will cause a crash.";
        if (ld.Volumes.OfType<Goal>().Count() == 1)
            yield return "The LevelDesc has only one Goal. This will cause a crash in Horde.";

        static IEnumerable<string> assetNameCheck(Level l, string brawlPath)
        {
            LevelDesc ld = l.Desc;
            // AssetDir
            string assetDir = ld.AssetDir;
            string assetDirPath = Path.Combine(brawlPath, "mapArt", assetDir);
            if (!WmeUtils.IsSubPathOf(assetDirPath, brawlPath))
            {
                yield return $"LevelDesc has AssetDir of \"{assetDir}\", which would end up outside the brawlhalla path";
                yield break; // subsequent errors would be a result of this
            }

            // backgrounds
            foreach (Background background in ld.Backgrounds)
            {
                // AssetName
                string assetName = background.AssetName;
                string assetNameExtension = Path.GetExtension(assetName);
                if (assetNameExtension != ".png" && assetNameExtension != ".jpg")
                    yield return $"A Background has AssetName of \"{assetName}\", but the game only supports png and jpg";
                string assetNamePath = Path.Combine(brawlPath, "Backgrounds", assetName);
                if (!WmeUtils.IsSubPathOf(assetNamePath, brawlPath))
                    yield return $"A Background has AssetName of \"{assetName}\", which would end up outside the brawlhalla path";
                // AnimatedAssetName
                string? animatedAssetName = background.AnimatedAssetName;
                if (animatedAssetName is not null)
                {
                    string animatedAssetNameExtension = Path.GetExtension(animatedAssetName);
                    if (animatedAssetNameExtension != ".png" && animatedAssetNameExtension != ".jpg")
                        yield return $"A Background has AnimatedAssetName of \"{animatedAssetName}\", but the game only supports png and jpg";
                    string animatedAssetNamePath = Path.Combine(brawlPath, "Backgrounds", animatedAssetName);
                    if (!WmeUtils.IsSubPathOf(animatedAssetNamePath, brawlPath))
                        yield return $"A Background has AnimatedAssetName of \"{animatedAssetName}\", which would end up outside the brawlhalla path";
                }
            }

            // assets
            foreach (AbstractAsset asset in ld.Assets)
            {
                foreach (string warning in checkAsset(asset, assetDirPath, brawlPath))
                    yield return warning;
            }

            static IEnumerable<string> checkAsset(AbstractAsset asset, string assetDirPath, string brawlPath)
            {
                string? assetName = asset.AssetName;
                if (assetName is not null)
                {
                    string assetNamePath = Path.Combine(assetDirPath, assetName);
                    string assetNameExtension = Path.GetExtension(assetName);
                    if (assetNameExtension != ".png" && assetNameExtension != ".jpg")
                        yield return $"A Background has AssetName of \"{assetName}\", but the game only supports png and jpg";
                    if (!WmeUtils.IsSubPathOf(assetNamePath, brawlPath))
                        yield return $"A {asset.GetType().Name} has AssetName of \"{assetName}\", which would end up outside the brawlhalla path";
                }

                if (asset is Platform platform && platform.AssetChildren is not null)
                {
                    foreach (AbstractAsset child in platform.AssetChildren)
                    {
                        foreach (string warning in checkAsset(child, assetDirPath, brawlPath))
                            yield return warning;
                    }
                }
                else if (asset is MovingPlatform mp)
                {
                    foreach (AbstractAsset child in mp.Assets)
                    {
                        foreach (string warning in checkAsset(child, assetDirPath, brawlPath))
                            yield return warning;
                    }
                }
            }
        }

        string? brawlPath = prefs.BrawlhallaPath;
        if (brawlPath is not null)
        {
            if (lt is not null && lt.ThumbnailPNGFile is not null)
            {
                string thumbnail = lt.ThumbnailPNGFile;
                string thumbnailExtension = Path.GetExtension(thumbnail);

                if (thumbnailExtension != ".png" && thumbnailExtension != ".jpg")
                    yield return $"The LevelType has ThumbnailPNGFile of \"{thumbnail}\", but the game only supports png and jpg";
                string thumbnailPath = Path.Combine(brawlPath, "images/thumbnails", thumbnail);
                if (!WmeUtils.IsSubPathOf(thumbnailPath, brawlPath))
                    yield return $"The LevelType has ThumbnailPNGFile of \"{thumbnailPath}\", which would end up outside the brawlhalla path";
            }

            foreach (string warning in assetNameCheck(l, brawlPath))
                yield return warning;
        }

        static IEnumerable<string> colliderBugCheck(Level l)
        {
            LevelDesc ld = l.Desc;

            HashSet<string> dynamicCollisionPlatIds = [];
            foreach (DynamicCollision dc in ld.DynamicCollisions)
            {
                dynamicCollisionPlatIds.Add(dc.PlatID);
            }

            foreach (MovingPlatform mp in ld.Assets.OfType<MovingPlatform>())
            {
                if (!dynamicCollisionPlatIds.Contains(mp.PlatID))
                {
                    yield return $"PlatID {mp.PlatID} does not move any DynamicCollision. This will cause a crash when a power with TargetMethod Collider is used.";
                }
            }
        }

        foreach (string warning in colliderBugCheck(l))
            yield return warning;
    }
}