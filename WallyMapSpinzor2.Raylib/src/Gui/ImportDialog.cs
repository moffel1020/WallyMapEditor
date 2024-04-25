using System.IO;
using System.Threading.Tasks;

using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public class ImportDialog(Editor editor) : IDialog
{
    private static string? lastLdPath;
    private static string? lastLtPath;
    private static string? lastLstPath;

    private string? _loadingError;

    private bool _open = true;
    public bool Closed => !_open;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 300), new(int.MaxValue));
        ImGui.Begin("Import", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        ImGui.BeginTabBar("importTabBar", ImGuiTabBarFlags.None);
        if (ImGui.BeginTabItem("Brawlhalla"))
        {
            ImGui.Text("Import from from game swz files");
            // TODO
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("xml"))
        {
            ShowXmlImportTab();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
        ImGui.End();
    }

    private void ShowXmlImportTab()
    {
        ImGui.Text("Import from LevelDesc xml file, LevelTypes.xml, and LevelSetTypes.xml");
        ImGui.Text("If LevelTypes.xml is not selected or it does not contain the level\na default LevelType will be generated");
        ImGui.SeparatorText("Select files");

        if (ImGui.Button("LevelDesc"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(filterList: "xml", defaultPath: Path.GetDirectoryName(lastLdPath));
                if (result.IsOk)
                    lastLdPath = result.Path;
            });
        }
        ImGui.SameLine();
        ImGui.Text(lastLdPath ?? "None");

        if (ImGui.Button("LevelTypes.xml (optional)"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(filterList: "xml", defaultPath: Path.GetDirectoryName(lastLtPath));
                if (result.IsOk)
                    lastLtPath = result.Path;
            });
        }
        ImGui.SameLine();
        if (lastLtPath is not null && ImGui.Button("x##lt")) lastLtPath = null;
        ImGui.SameLine();
        ImGui.Text(lastLtPath ?? "None");

        if (ImGui.Button("LevelSetTypes.xml (optional)"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen(filterList: "xml", defaultPath: Path.GetDirectoryName(lastLstPath));
                if (result.IsOk)
                    lastLstPath = result.Path;
            });
        }
        ImGui.SameLine();
        if (lastLstPath is not null && ImGui.Button("x##lst")) lastLstPath = null;
        ImGui.SameLine();
        ImGui.Text(lastLstPath ?? "None");

        ImGui.Separator();
        if (lastLdPath is not null && ImGui.Button("Import"))
        {
            try
            {
                editor.LoadMap(lastLdPath, lastLtPath, lastLstPath);
                _open = false;
                _loadingError = null;
            }
            catch (System.Xml.XmlException e)
            {
                Rl.TraceLog(TraceLogLevel.Error, e.Message);
                _loadingError = $"Could not load xml file. {e.Message}";
            }
        }

        if (_loadingError is not null) ImGui.Text($"Error: {_loadingError}");
    }
}