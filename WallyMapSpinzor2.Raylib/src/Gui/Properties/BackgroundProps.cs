using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowBackgroundProps(Background b, CommandHistory cmd, PropertiesWindowData data)
    {
        string? backgroundDir = data.PathPrefs.BrawlhallaPath is not null
            ? Path.Combine(data.PathPrefs.BrawlhallaPath, "mapArt", "Backgrounds")
            : null;

        bool propChanged = false;
        ImGui.Text("AssetName: " + b.AssetName);
        if (backgroundDir is not null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Select##AssetName"))
            {
                Task.Run(() =>
                {
                    DialogResult dialogResult = Dialog.FileOpen("png,jpg", backgroundDir);
                    if (dialogResult.IsOk)
                    {
                        string path = dialogResult.Path;
                        string newAssetName = Path.GetRelativePath(backgroundDir, path);
                        if (newAssetName != b.AssetName)
                        {
                            cmd.Add(new PropChangeCommand<string>(val => b.AssetName = val, b.AssetName, newAssetName));
                            propChanged = true;
                        }
                    }
                });
            }
            if (data.Canvas is not null)
            {
                Texture2DWrapper texture = data.Canvas.LoadTextureFromPath(Path.Combine(backgroundDir, b.AssetName));
                rlImGui.ImageSize(texture.Texture, new Vector2(200 * (float)(texture.Width / texture.Height), 200));
            }
        }
        ImGui.Text("AnimatedAssetName: " + (b.AnimatedAssetName ?? "None"));
        if (backgroundDir is not null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Select##AnimatedAssetName"))
            {
                Task.Run(() =>
                {
                    DialogResult dialogResult = Dialog.FileOpen("png,jpg", backgroundDir);
                    if (dialogResult.IsOk)
                    {
                        string path = dialogResult.Path;
                        string newAnimatedAssetName = Path.GetRelativePath(backgroundDir, path);
                        if (newAnimatedAssetName != b.AnimatedAssetName)
                        {
                            cmd.Add(new PropChangeCommand<string?>(val => b.AnimatedAssetName = val, b.AnimatedAssetName, newAnimatedAssetName));
                            propChanged = true;
                        }
                    }
                });
            }
            ImGui.SameLine();
            if (ImGuiExt.WithDisabledButton(b.AnimatedAssetName is null, "Remove##AnimatedAssetName"))
            {
                if (b.AnimatedAssetName is not null)
                {
                    cmd.Add(new PropChangeCommand<string?>(val => b.AnimatedAssetName = val, b.AnimatedAssetName, null));
                    propChanged = true;
                }
            }
            if (data.Canvas is not null && b.AnimatedAssetName is not null)
            {
                Texture2DWrapper texture = data.Canvas.LoadTextureFromPath(Path.Combine(backgroundDir, b.AnimatedAssetName));
                rlImGui.ImageSize(texture.Texture, new Vector2(200 * (float)(texture.Width / texture.Height), 200));
            }
        }
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