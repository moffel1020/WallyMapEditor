using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WallyMapSpinzor2;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public static class PlaylistEditPanel
{
    private static bool _open;
    public static bool Open
    {
        get => _open;
        set
        {
            _open = value;
            _levelSetTypes = null;
            _allPlaylists = [];
        }
    }

    private static LevelSetTypes? _levelSetTypes;
    private static string[] _allPlaylists = [];

    private static string? _gameSwzPath;
    private static string? _decyptionKey;

    private static int _importMethod;
    private static string? _swzImportError;
    private static string _playlistFilter = "";

    public static void Show(Level l, PathPreferences prefs)
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 410), new(int.MaxValue));
        ImGui.Begin("Playlist editor", ref _open, ImGuiWindowFlags.NoDocking);

        ImGui.Text("Import from:");
        ImGui.RadioButton("Game.swz", ref _importMethod, 0);
        ImGui.RadioButton("LevelSetTypes.xml", ref _importMethod, 1);
        ImGui.Separator();

        if (_importMethod == 0)
        {
            if (_gameSwzPath is null && prefs.BrawlhallaPath is not null)
                _gameSwzPath = Path.Combine(prefs.BrawlhallaPath, "Game.swz");

            ImGui.Text("Path: " + _gameSwzPath);
            if (ImGui.Button("Select Game.swz"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                    if (result.IsOk) _gameSwzPath = result.Path;
                });
            }

            _decyptionKey = ImGuiExt.InputText("Decryption key", _decyptionKey ?? prefs.DecryptionKey ?? "", ImportWindow.MAX_KEY_LENGTH, ImGuiInputTextFlags.CharsDecimal);
            if (ImGuiExt.WithDisabledButton(string.IsNullOrEmpty(_gameSwzPath) || string.IsNullOrEmpty(_decyptionKey), "Import##swz"))
            {
                try
                {
                    uint key = uint.Parse(_decyptionKey!);
                    _levelSetTypes = WmeUtils.DeserializeSwzFromPath<LevelSetTypes>(_gameSwzPath!, "LevelSetTypes.xml", key, bhstyle: true);
                    if (_levelSetTypes is null) throw new Exception("Could not deserialize Game.swz");
                    _allPlaylists = _levelSetTypes.Playlists.Select(lst => lst.LevelSetName).Where(p => p != "Auto").Distinct().ToArray();
                    l.Playlists = l.Playlists.Where(p => _allPlaylists.Contains(p)).ToHashSet();
                    _swzImportError = null;
                    _playlistFilter = "";
                }
                catch (Exception e)
                {
                    _swzImportError = e.Message;
                }
            }

            if (_swzImportError is not null)
                ImGui.TextWrapped("[Error]: " + _swzImportError);

        }
        else if (_importMethod == 1)
        {
            ImGui.Text("Path: " + prefs.LevelSetTypesPath);
            if (ImGui.Button("Select LevelSetTypes.xml"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                    if (result.IsOk) prefs.LevelSetTypesPath = result.Path;
                });
            }

            if (prefs.LevelSetTypesPath is not null && ImGui.Button("Import##lst"))
            {
                _levelSetTypes = WmeUtils.DeserializeFromPath<LevelSetTypes>(prefs.LevelSetTypesPath, bhstyle: true);
                _allPlaylists = _levelSetTypes.Playlists.Select(lst => lst.LevelSetName).Where(p => p != "Auto").Distinct().ToArray();
                l.Playlists = l.Playlists.Where(p => _allPlaylists.Contains(p)).ToHashSet();
                _playlistFilter = "";
            }
        }

        if (_allPlaylists.Length != 0)
        {
            ImGui.SeparatorText($"Playlists ({l.Playlists.Count})");
            ImGui.InputText("##playlistfilter", ref _playlistFilter, 100);
            if (_playlistFilter != "")
            {
                ImGui.SameLine();
                if (ImGui.Button("x")) _playlistFilter = "";
            }
            ImGui.SameLine();
            ImGui.Text("Filter");

            string[] playlists = _allPlaylists.Where(p => p.Contains(_playlistFilter, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            foreach (string playlist in playlists)
            {
                bool contained = l.Playlists.Contains(playlist);
                if (ImGui.Checkbox(playlist, ref contained))
                {
                    if (contained)
                        l.Playlists.Add(playlist);
                    else
                        l.Playlists.Remove(playlist);
                }
            }
        }

        ImGui.End();
    }
}