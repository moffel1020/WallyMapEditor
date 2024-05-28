using System.IO;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

public class InitialSetupWindow()
{
    private static string? _selectError;

    public static void ShowInitialFileSelect(PathPreferences prefs)
    {
        ImGui.Begin("Setup", ImGuiWindowFlags.NoCollapse);

        if (ImGui.Button("Select brawlhalla path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(prefs.BrawlhallaPath);
                if (result.IsOk)
                {
                    if (Utils.IsValidBrawlPath(result.Path))
                    {
                        prefs.BrawlhallaPath = result.Path;
                        prefs.BrawlhallaAirPath ??= Path.Combine(prefs.BrawlhallaPath, "BrawlhallaAir.swf");
                        prefs.Save();
                        _selectError = null;
                    }
                    else
                    {
                        _selectError = "Selected path is not a valid brawlhalla path";
                    }
                }
            });
        }

        if (_selectError is not null)
        {
            ImGui.Text("[Error]: " + _selectError);
        }

        ImGui.End();
    }
}