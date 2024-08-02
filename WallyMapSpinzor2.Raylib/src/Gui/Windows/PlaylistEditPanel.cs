using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

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
        }
    }

    private static LevelSetTypes? _levelSetTypes;
    public static string[] AllPlaylist { get; private set; } = [];

    private static int _importMethod;

    public static void Show(Level l, PathPreferences prefs)
    {
        ImGui.Begin("Playlist editor", ref _open, ImGuiWindowFlags.NoDocking);

        ImGui.Text("Import from:");
        ImGui.RadioButton("Game", ref _importMethod, 0);
        ImGui.RadioButton("LevelSetTypes.xml", ref _importMethod, 1);
        ImGui.Separator();

        if (_importMethod == 0)
        {
            // TODO
        }
        else if (_importMethod == 1)
        {
            ImGui.Text("Path: " + prefs.LevelSetTypesPath);
            if (ImGui.Button("Select LevelSetTypes.xml"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelSetTypesPath));
                    if (result.IsOk)
                    {
                        ImportPlaylistXmlFromPath(l, result.Path);
                        prefs.LevelSetTypesPath = result.Path;
                    }
                });
            }

            if (prefs.LevelSetTypesPath is not null && ImGui.Button("Import"))
                ImportPlaylistXmlFromPath(l, prefs.LevelSetTypesPath);
        }

        if (AllPlaylist.Length != 0)
        {
            ImGui.SeparatorText("Playlists");
            foreach (string playlist in AllPlaylist)
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

    private static void ImportPlaylistXmlFromPath(Level l, string path)
    {
        _levelSetTypes = Wms2RlUtils.DeserializeFromPath<LevelSetTypes>(path);
        AllPlaylist = _levelSetTypes.Playlists.Select(lst => lst.LevelSetName).Distinct().ToArray(); 
        l.Playlists = l.Playlists.Where(p => AllPlaylist.Contains(p)).ToHashSet();
    }
}