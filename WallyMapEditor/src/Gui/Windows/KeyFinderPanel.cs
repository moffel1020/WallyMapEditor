using System;
using System.IO;
using System.Threading.Tasks;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapEditor;

public static class KeyFinderPanel
{
    private static string? _foundKey = null;
    private static string? _errorMessage = null;
    private static bool _open = false;
    public static bool Open { get => _open; set => _open = value; }

    public static void Show(PathPreferences prefs)
    {
        ImGui.Begin("Swz Key Finder", ref _open, ImGuiWindowFlags.NoDocking);

        if (ImGui.Button("Select BrawlhallaAir.swf"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("swf", Path.GetDirectoryName(prefs.BrawlhallaPath));
                if (result.IsOk)
                    prefs.BrawlhallaAirPath = result.Path;
            });
        }
        if (prefs.BrawlhallaPath is null)
            ImGui.TextColored(ImGuiExt.RGBHexToVec4(0xAA4433), "Please select path");
        else
            ImGui.Text($"Selected path: {prefs.BrawlhallaAirPath}");

        if (!string.IsNullOrWhiteSpace(prefs.BrawlhallaAirPath) && ImGui.Button("Find"))
        {
            Task.Run(() =>
            {
                try
                {
                    _foundKey = WmeUtils.FindDecryptionKeyFromPath(prefs.BrawlhallaAirPath)?.ToString();
                    _errorMessage = null;
                }
                catch (Exception e)
                {
                    _errorMessage = e.Message;
                }
            });
        }

        if (_foundKey is not null)
        {
            ImGui.Separator();
            ImGuiExt.InputText("found key", _foundKey, flags: ImGuiInputTextFlags.ReadOnly);
        }

        if (!string.IsNullOrEmpty(_errorMessage))
            ImGui.Text($"[Error] {_errorMessage}");

        ImGui.End();
    }
}