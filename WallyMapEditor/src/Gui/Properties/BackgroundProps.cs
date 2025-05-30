using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using WallyMapSpinzor2;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;
using System;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    private static string? _warningText;
    private static string? _backgroundErrorText;

    public static bool ShowBackgroundProps(Background b, EditorLevel level, PropertiesWindowData data)
    {
        CommandHistory cmd = level.CommandHistory;

        string? backgroundDir = data.PathPrefs.BrawlhallaPath is not null
            ? Path.Combine(data.PathPrefs.BrawlhallaPath, "mapArt", "Backgrounds")
            : null;

        bool propChanged = false;
        ImGui.Text("AssetName: " + b.AssetName);

        if (data.PathPrefs.BrawlhallaPath is null) return false;

        if (backgroundDir is not null)
        {
            ImGui.SameLine();
            ShowSelectBackgroundButton("AssetName", b.AssetName, val => b.AssetName = val!, cmd, data, backgroundDir);
            if (data.Loader is not null)
            {
                Texture2DWrapper texture = data.Loader.LoadTextureFromPath(Path.Combine(backgroundDir, b.AssetName));
                if (texture.Texture.Id != 0)
                {
                    rlImGui.ImageSize(texture.Texture, new Vector2(200.0f * texture.Width / texture.Height, 200));
                    (b.W, b.H) = (texture.Texture.Width, texture.Texture.Height);
                }
            }
        }
        ImGui.Text("AnimatedAssetName: " + (b.AnimatedAssetName ?? "None"));
        if (backgroundDir is not null)
        {
            ImGui.SameLine();
            ShowSelectBackgroundButton("AnimatedAssetName", b.AnimatedAssetName, val => b.AnimatedAssetName = val, cmd, data, backgroundDir);
            ImGui.SameLine();
            if (ImGuiExt.ButtonDisabledIf(b.AnimatedAssetName is null, "Remove##AnimatedAssetName"))
            {
                if (b.AnimatedAssetName is not null)
                {
                    cmd.Add(new PropChangeCommand<string?>(val => b.AnimatedAssetName = val, b.AnimatedAssetName, null));
                    propChanged = true;
                }
            }
            if (data.Loader is not null && b.AnimatedAssetName is not null)
            {
                Texture2DWrapper texture = data.Loader.LoadTextureFromPath(Path.Combine(backgroundDir, b.AnimatedAssetName));
                if (texture.Texture.Id != 0)
                {
                    rlImGui.ImageSize(texture.Texture, new Vector2(200.0f * texture.Width / texture.Height, 200));
                    if (texture.Width != b.W || texture.Height != b.H)
                        _warningText = "AnimatedAssetName image is not the same size as the AssetName image. This can lead to the image displaying incorrectly";
                    else
                        _warningText = null;
                }
            }
        }
        ImGuiExt.HintTooltip(Strings.UI_BG_ANIMATED_ASSET_TOOLTIP);

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

        ImGui.PushTextWrapPos();
        if (_warningText is not null) ImGui.Text($"[Warning]: {_warningText}");
        if (_backgroundErrorText is not null) ImGui.Text($"[Error]: {_backgroundErrorText}");
        ImGui.PopTextWrapPos();

        return propChanged;
    }

    private static void ShowSelectBackgroundButton(string key, string? assetName, Action<string?> setAssetName, CommandHistory cmd, PropertiesWindowData data, string backgroundDir)
    {
        if (data.PathPrefs.BrawlhallaPath is null || !ImGui.Button($"Select##{key}")) return;

        Task.Run(() =>
        {
            DialogResult dialogResult = Dialog.FileOpen("png,jpg", backgroundDir);
            if (dialogResult.IsOk)
            {
                string path = dialogResult.Path;
                string newAssetName = Path.GetRelativePath(backgroundDir, path).Replace("\\", "/");
                if (!WmeUtils.IsInDirectory(data.PathPrefs.BrawlhallaPath, path))
                {
                    _backgroundErrorText = "Asset has to be inside brawlhalla directory";
                    return;
                }

                if (newAssetName == assetName)
                {
                    _backgroundErrorText = null;
                    return;
                }

                string? extension = Path.GetExtension(newAssetName);
                if (extension != ".png" && extension != ".jpg")
                {
                    _backgroundErrorText = "Background file must be .png or .jpg";
                    return;
                }

                cmd.Add(new PropChangeCommand<string?>(setAssetName, assetName, newAssetName));
                _backgroundErrorText = null;
            }
        });
    }
}