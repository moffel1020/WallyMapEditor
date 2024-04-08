namespace WallyMapSpinzor2.Raylib;

using System.Numerics;
using System.Threading.Tasks;
using System.IO;
using ImGuiNET;
using WallyMapSpinzor2;
using NativeFileDialogSharp;

public class ExportDialog(IDrawable? mapData) : IDialog
{

    private static string? lastPath;

    public bool _open = true;
    public bool Closed { get => !_open; }
    private readonly IDrawable? _mapData = mapData;

    private string? _descPreview;
    private string? _typePreview;

    private const int PREVIEW_SIZE = 25;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Export", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        if (_mapData is null)
        {
            ImGui.Text("No map data open");
            return;
        }

        LevelDesc? ld = _mapData switch {
            Level level => level.Desc,
            LevelDesc desc => desc,
            _ => null
        };

        ImGui.BeginTabBar("exportTabBar", ImGuiTabBarFlags.None);

        if (ld is not null && ImGui.BeginTabItem("LevelDesc"))
        {
            ImGui.Text("preview");

            if (_descPreview is not null)
                ImGui.InputTextMultiline("leveldesc##preview", ref _descPreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
            else if (ImGui.Button("Generate preview")) 
                _descPreview = Utils.SerializeToString(ld);

            if (ImGui.Button("Export"))
            {
                Task.Run(() => 
                {
                    DialogResult result = Dialog.FileSave(filterList: "xml", defaultPath: lastPath);
                    if (result.IsOk) 
                    {
                        Utils.SerializeToPath(ld, result.Path);
                        lastPath = Path.GetDirectoryName(result.Path);
                    }
                });
            }
            ImGui.EndTabItem();
        }

        if (_mapData is Level l)
        {
            if (l.Type is not null && ImGui.BeginTabItem("LevelType"))
            {
                ImGui.Text("preview");

                if (_typePreview is not null)
                    ImGui.InputTextMultiline("leveltype##preview", ref _typePreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
                else if (ImGui.Button("Generate preview")) 
                    _typePreview = Utils.SerializeToString(l.Type);

                // TODO: exporting this by itself is kinda pointless, add the options to add this leveltype to leveltypes.xml
                if (ImGui.Button("Export"))
                {
                    Task.Run(() => 
                    {
                        DialogResult result = Dialog.FileSave(filterList: "xml", defaultPath: lastPath);
                        if (result.IsOk) 
                        {
                            Utils.SerializeToPath(l.Type, result.Path);
                            lastPath = Path.GetDirectoryName(result.Path);
                        }
                    });
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Playlists"))
            {
                ImGui.Text($"{l.Desc.LevelName} is in playlists:");
                foreach (string playlist in l.Playlists)
                    ImGui.BulletText(playlist);

                ImGui.EndTabItem();
            }
        }
        ImGui.EndTabBar();

        ImGui.End();
    }
}