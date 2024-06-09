using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowBackgroundProps(Background b, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("AssetName: " + b.AssetName);
        ImGui.Text("AnimatedAssetName: " + (b.AnimatedAssetName ?? "None"));
        ImGui.Text("W: " + b.W);
        ImGui.Text("H: " + b.H);
        propChanged |= ImGuiExt.CheckboxHistory("HasSkulls", b.HasSkulls, val => b.HasSkulls = val, cmd);
        if (b.Theme is null)
        {
            ImGui.Text("No theme");
        }
        else
        {
            ImGui.Text("Themes:");
            foreach (string theme in b.Theme)
            {
                ImGui.BulletText(theme);
            }
        }
        return propChanged;
    }
}