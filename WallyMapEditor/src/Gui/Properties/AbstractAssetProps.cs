using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using WallyMapSpinzor2;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;

namespace WallyMapEditor;

partial class PropertiesWindow
{
    private static string? _assetErrorText;

    public static bool ShowAbstractAssetProps(AbstractAsset a, EditorLevel level, PropertiesWindowData data)
    {
        SelectionContext selection = level.Selection;
        CommandHistory cmd = level.CommandHistory;

        if (a.Parent is not null)
        {
            ImGui.Text($"Parent {a.Parent.GetType().Name}: ");
            ImGui.SameLine();
            if (ImGui.Button($"{MapOverviewWindow.GetExtraObjectInfo(a.Parent)}"))
                selection.Object = a.Parent;
            ImGui.Separator();
        }

        bool propChanged = false;

        RemoveButton(a, level);
        ImGui.Separator();

        if (a.AssetName is not null)
        {
            ImGui.Text("AssetName: " + a.AssetName);

            if (data.PathPrefs.BrawlhallaPath is not null)
            {
                string assetDir = Path.Combine(data.PathPrefs.BrawlhallaPath, "mapArt", level.Level.Desc.AssetDir);
                ImGui.SameLine();
                ShowSelectAssetButton(a, level, data, assetDir);

                if (_assetErrorText is not null)
                {
                    ImGui.TextWrapped("[Error]: " + _assetErrorText);
                }

                if (data.Loader is not null)
                {
                    Texture2DWrapper texture = data.Loader.LoadTextureFromPath(Path.Combine(assetDir, a.AssetName));
                    if (texture.Texture.Id != 0)
                    {
                        rlImGui.ImageSize(texture.Texture, new Vector2(60.0f * texture.Width / texture.Height, 60));
                    }

                    if (ImGui.Button("Reset width/height"))
                    {
                        if (data.Loader.TextureCache.Cache.TryGetValue(Path.Combine(assetDir, a.AssetName), out Texture2DWrapper? tex))
                        {
                            cmd.Add(new PropChangeCommand<double, double>(
                                (val1, val2) => (a.W, a.H) = (val1, val2),
                                a.W!.Value, a.H!.Value,
                                tex.Width, tex.Height
                            ));
                        }
                    }
                }
            }
            propChanged |= ImGuiExt.DragDoubleHistory("X", a.X, val => a.X = val, cmd);
            propChanged |= ImGuiExt.DragDoubleHistory("Y", a.Y, val => a.Y = val, cmd);
            ImGui.Separator();
            // proper editing of W and H alongside ScaleX and ScaleY is messy. so just do this.
            propChanged |= ImGuiExt.DragDoubleHistory("W", a.W ?? 0, val => a.W = val, cmd);
            propChanged |= ImGuiExt.DragDoubleHistory("H", a.H ?? 0, val => a.H = val, cmd);
            ImGui.Separator();
            propChanged |= ImGuiExt.DragDoubleHistory("Rotation", a.Rotation, val => a.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1f);
        }
        else
        {
            propChanged |= ImGuiExt.DragDoubleHistory("X", a.X, val => a.X = val, cmd);
            propChanged |= ImGuiExt.DragDoubleHistory("Y", a.Y, val => a.Y = val, cmd);
            ImGui.Separator();
            propChanged |= ImGuiExt.DragDoubleHistory("ScaleX", a.ScaleX, val => a.ScaleX = val, cmd, speed: 0.01f);
            propChanged |= ImGuiExt.DragDoubleHistory("ScaleY", a.ScaleY, val => a.ScaleY = val, cmd, speed: 0.01f);
            ImGui.Separator();
            propChanged |= ImGuiExt.DragDoubleHistory("Rotation", a.Rotation, val => a.Rotation = BrawlhallaMath.SafeMod(val, 360.0), cmd, speed: 0.1f);
        }
        return propChanged;
    }

    private static void ShowSelectAssetButton(AbstractAsset a, EditorLevel level, PropertiesWindowData data, string assetDir)
    {
        if (data.PathPrefs.BrawlhallaPath is null || !ImGui.Button("Select##AssetName")) return;

        Task.Run(() =>
        {
            DialogResult dialogResult = Dialog.FileOpen("png,jpg", assetDir);
            if (dialogResult.IsOk)
            {
                string path = dialogResult.Path;
                string newAssetName = Path.GetRelativePath(assetDir, path).Replace("\\", "/");
                if (!WmeUtils.IsInDirectory(data.PathPrefs.BrawlhallaPath, path))
                {
                    _assetErrorText = "Asset has to be inside brawlhalla directory";
                    return;
                }

                if (newAssetName == a.AssetName)
                {
                    _assetErrorText = null;
                    return;
                }

                string? extension = Path.GetExtension(newAssetName);
                if (extension != ".png" && extension != ".jpg")
                {
                    _assetErrorText = "Asset file must be .png or .jpg";
                    return;
                }

                level.CommandHistory.Add(new PropChangeCommand<string>(val => a.AssetName = val, a.AssetName!, newAssetName));
                _assetErrorText = null;
            }
        });
    }

    public static Asset DefaultAsset(double posX, double posY) => new()
    {
        AssetName = "../BattleHill/SK_Small_Plat.png",
        X = posX,
        Y = posY,
        W = 750,
        H = 175,
    };
}