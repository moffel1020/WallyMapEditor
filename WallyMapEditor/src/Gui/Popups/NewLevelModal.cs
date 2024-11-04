using System.IO;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public static class NewLevelModal
{
    public const string NAME = "Create a new level";

    private static bool _shouldOpen = false;
    private static bool _open = false;
    public static void Open() => _shouldOpen = true;

    private static string _newLevelName = "";
    private static string? _newLevelDir = null;
    private static string _newDisplayName = "";
    private static bool _addToPlaylists = true;

    private static string? _levelDirSelectError = null;

    public static void Update(LevelLoader loader, PathPreferences prefs)
    {
        if (_shouldOpen)
        {
            ImGui.OpenPopup(NAME);
            _shouldOpen = false;
            _open = true;
            _levelDirSelectError = null;
        }
        if (!ImGui.BeginPopupModal(NAME, ref _open)) return;

        ImGui.Text("Pick a name for the new level");
        ImGui.Text("These settings can always be changed later");
        _newLevelName = ImGuiExt.InputTextWithFilter("LevelName", _newLevelName, 64);
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Unique name of the level, this will be used as the name of the asset folder.\nIf another map exists with this LevelName, it will be overwritten.");

        if (prefs.BrawlhallaPath is not null)
        {
            if (ImGui.Button("Select AssetDir"))
            {
                string mapArtPath = Path.Combine(prefs.BrawlhallaPath, "mapArt");
                Task.Run(() =>
                {
                    DialogResult dialogResult = Dialog.FolderPicker(mapArtPath);
                    if (dialogResult.IsOk)
                    {
                        string path = dialogResult.Path;
                        string dir = Path.GetRelativePath(mapArtPath, path).Replace("\\", "/");
                        if (!WmeUtils.IsInDirectory(prefs.BrawlhallaPath, path))
                        {
                            _levelDirSelectError = "AssetDir has to be inside the brawlhalla directory";
                        }
                        else
                        {
                            _newLevelDir = dir;
                        }
                    }
                });
            }
            if (_newLevelDir is not null)
            {
                ImGui.SameLine();
                ImGui.Text(_newLevelDir);
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("The folder name to take assets from. Leave empty to use the LevelName.");
        }
        if (_levelDirSelectError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("[Error]: " + _levelDirSelectError);
            ImGui.PopTextWrapPos();
        }

        ImGui.InputText("DisplayName", ref _newDisplayName, 64);
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Name shown in game when selecting a level");

        ImGui.Checkbox("Add default playlists", ref _addToPlaylists);

        if (ImGui.Button("Brawlhalla directory: "))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                if (result.IsOk && WmeUtils.IsValidBrawlPath(result.Path))
                    prefs.BrawlhallaPath = result.Path;
            });
        }
        ImGui.SameLine();
        ImGui.Text(prefs.BrawlhallaPath ?? "Not selected");

        if (ImGuiExt.WithDisabledButton(string.IsNullOrWhiteSpace(_newDisplayName) || string.IsNullOrWhiteSpace(_newLevelName) || string.IsNullOrWhiteSpace(prefs.BrawlhallaPath), "Create"))
        {
            string assetDir = string.IsNullOrWhiteSpace(_newLevelDir) ? _newLevelName : _newLevelDir;
            loader.LoadDefaultMap(_newLevelName, assetDir, _newDisplayName, _addToPlaylists);
            string dir = Path.Combine(prefs.BrawlhallaPath!, "mapArt", assetDir);
            Directory.CreateDirectory(dir);

            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
}