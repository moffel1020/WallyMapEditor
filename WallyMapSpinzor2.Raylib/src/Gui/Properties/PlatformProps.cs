using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowPlatformProps(Platform p, CommandHistory cmd, RaylibCanvas? canvas, string? assetDir)
    {
        bool propChanged = false;

        string name = p.InstanceName;
        ImGui.InputText("InstanceName", ref name, 64);
        if (name != p.InstanceName)
        {
            cmd.Add(new PropChangeCommand<string>(val => p.InstanceName = val, p.InstanceName, name));
            propChanged = true;
        }

        ImGui.Separator();
        propChanged |= ShowAbstractAssetProps(p, cmd, canvas, assetDir);
        if (p.AssetName is not null)
        {
            ImGui.Text("PlatformAssetSwap: " + (p.PlatformAssetSwap ?? "None"));
            if (assetDir is not null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Select"))
                {
                    Task.Run(() =>
                    {
                        DialogResult dialogResult = Dialog.FileOpen("png,jpg", assetDir);
                        if (dialogResult.IsOk)
                        {
                            string path = dialogResult.Path;
                            string newPlatformAssetSwap = Path.GetRelativePath(assetDir, path);
                            if (newPlatformAssetSwap != p.PlatformAssetSwap)
                            {
                                cmd.Add(new PropChangeCommand<string?>(val => p.PlatformAssetSwap = val, p.PlatformAssetSwap, newPlatformAssetSwap));
                                propChanged = true;
                            }
                        }
                    });
                }
                ImGui.SameLine();
                if (ImGuiExt.WithDisabledButton(p.PlatformAssetSwap is null, "Remove"))
                {
                    if (p.PlatformAssetSwap is not null)
                    {
                        cmd.Add(new PropChangeCommand<string?>(val => p.PlatformAssetSwap = val, p.PlatformAssetSwap, null));
                        propChanged = true;
                    }
                }

                if (p.PlatformAssetSwap is not null && canvas is not null)
                {
                    Texture2DWrapper texture = canvas.LoadTextureFromPath(Path.Combine(assetDir, p.PlatformAssetSwap));
                    rlImGui.ImageSize(texture.Texture, new Vector2(60 * (float)(texture.Width / texture.Height), 60));
                }
            }
        }

        ImGui.Separator();
        ImGui.Text($"Blue: {p.Blue?.ToString() ?? "No"}");
        ImGui.Text($"Red: {p.Red?.ToString() ?? "No"}");

        if (p.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            foreach (AbstractAsset child in p.AssetChildren!)
            {
                if (ImGui.TreeNode($"{child.GetType().Name}##{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd, canvas, assetDir);
                    ImGui.TreePop();
                }
            }
        }

        return propChanged;
    }
}