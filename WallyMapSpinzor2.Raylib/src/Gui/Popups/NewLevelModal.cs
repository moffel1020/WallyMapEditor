using System.IO;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

public static class NewLevelModal
{
    public const string NAME = "Create a new level";

    private static bool _shouldOpen = false;
    private static bool _open = false;
    public static void Open() => _shouldOpen = true;

    private static string _newLevelName = "";
    private static string _newDisplayName = "";
    private static bool _addToPlaylists = true;

    public static void Update(Editor editor, PathPreferences prefs)
    {
        if (_shouldOpen)
        {
            ImGui.OpenPopup(NAME);
            _shouldOpen = false;
            _open = true;
        }
        if (!ImGui.BeginPopupModal(NAME, ref _open)) return;

        ImGui.Text("Pick a name for the new level");
        ImGui.Text("These settings can always be changed later");
        unsafe
        {
            _newLevelName = ImGuiExt.InputTextWithCallback("LevelName", _newLevelName, MapOverviewWindow.LevelNameFilter, 64);
        }
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Unique name of the level, this will be used as the name of the asset folder.\nIf another map exists with this LevelName, it will be overwritten.");

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
                if (result.IsOk && Wms2RlUtils.IsValidBrawlPath(result.Path))
                    prefs.BrawlhallaPath = result.Path;
            });
        }
        ImGui.SameLine();
        ImGui.Text(prefs.BrawlhallaPath ?? "Not selected");

        if (ImGuiExt.WithDisabledButton(string.IsNullOrWhiteSpace(_newDisplayName) || string.IsNullOrWhiteSpace(_newLevelName) || string.IsNullOrWhiteSpace(prefs.BrawlhallaPath), "Create"))
        {
            LevelDesc ld = Editor.DefaultLevelDesc;
            LevelType lt = Editor.DefaultLevelType;
            ld.AssetDir = ld.LevelName = lt.LevelName = _newLevelName;
            lt.DisplayName = _newDisplayName;
            Level level = new(ld, lt, _addToPlaylists ? [.. Editor.DefaultPlaylists] : []);
            // FIXME: cba to load bonenames properly here. might become problematic if we ever allow the user to add animations
            editor.LoadMapFromLevel(level, editor.BoneNames ?? [], editor.PowerNames);

            string dir = Path.Combine(prefs.BrawlhallaPath!, "mapArt", _newLevelName);
            Directory.CreateDirectory(dir);

            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
}