using ImGuiNET;

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
            propChanged |= ImGuiExt.GenericStringComboHistory("PlatformAssetSwap", p.PlatformAssetSwap, val => p.PlatformAssetSwap = val,
            s => s switch
            {
                "simple" or "animated" => s,
                _ => "always",
            },
            s => s switch
            {
                "simple" or "animated" => s,
                _ => null,
            },
            ["always", "simple", "animated"], cmd);
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