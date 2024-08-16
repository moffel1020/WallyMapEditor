using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using rlImGui_cs;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    private static string? _assetErrorText;

    public static bool ShowAbstractAssetProps(AbstractAsset a, CommandHistory cmd, PropertiesWindowData data)
    {
        if (a.Parent is not null)
        {
            ImGui.Text($"Parent {a.Parent.GetType().Name}: ");
            ImGui.SameLine();
            if (ImGui.Button($"{MapOverviewWindow.GetExtraObjectInfo(a.Parent)}")) data.Selection.Object = a.Parent;
            ImGui.Separator();
        }

        bool propChanged = false;
        if (a.AssetName is not null)
        {
            ImGui.Text("AssetName: " + a.AssetName);

            if (data.Level is not null && data.PathPrefs.BrawlhallaPath is not null)
            {
                string assetDir = Path.Combine(data.PathPrefs.BrawlhallaPath, "mapArt", data.Level.Desc.AssetDir);
                ImGui.SameLine();
                if (ImGui.Button("Select##AssetName"))
                {
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
                            }
                            else if (newAssetName != a.AssetName)
                            {
                                cmd.Add(new PropChangeCommand<string>(val => a.AssetName = val, a.AssetName, newAssetName));
                                propChanged = true;
                                _assetErrorText = null;
                            }
                        }
                    });
                }

                if (_assetErrorText is not null)
                {
                    ImGui.PushTextWrapPos();
                    ImGui.Text("[Error]: " + _assetErrorText);
                    ImGui.PopTextWrapPos();
                }

                if (data.Loader is not null)
                {
                    Texture2DWrapper texture = data.Loader.LoadTextureFromPath(Path.Combine(assetDir, a.AssetName));
                    rlImGui.ImageSize(texture.Texture, new Vector2(60 * (float)(texture.Width / texture.Height), 60));
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

    public static Asset DefaultAsset(Vector2 pos) => new()
    {
        AssetName = "../BattleHill/SK_Small_Plat.png",
        X = pos.X,
        Y = pos.Y,
        W = 750,
        H = 175,
    };
}