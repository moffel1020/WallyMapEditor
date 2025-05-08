using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

partial class PropertiesWindow
{
    public static bool ShowPlatformProps(Platform p, CommandHistory cmd, PropertiesWindowData data)
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
        propChanged |= ShowAbstractAssetProps(p, cmd, data);
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
            }, [null, "simple", "animated"], cmd);
            ImGuiExt.HintTooltip(Strings.UI_PLATFORM_ASSET_SWAP_TOOLTIP);
        }

        ImGui.Separator();
        ImGui.Text($"Blue: {p.Blue?.ToString() ?? "No"}");
        ImGui.Text($"Red: {p.Red?.ToString() ?? "No"}");

        if (p.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", p.AssetChildren!, val => p.AssetChildren = val,
            () => CreateNewPlatformChild(p),
            index =>
            {
                if (index != 0)
                    ImGui.Separator();
                AbstractAsset child = p.AssetChildren![index];
                bool changed = false;
                if (ImGui.TreeNode($"{child.GetType().Name}###{child.GetHashCode()}"))
                {
                    changed |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }

                if (ImGui.Button($"Select##platchild{child.GetHashCode()}") && data.Selection is not null)
                    data.Selection.Object = child;
                ImGui.SameLine();

                return changed;
            }, cmd);
        }

        return propChanged;
    }

    private static Maybe<AbstractAsset> CreateNewPlatformChild(Platform parent)
    {
        Maybe<AbstractAsset> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##platform");

        if (ImGui.BeginPopup("AddChild##platform"))
        {
            result = AddObjectPopup.AddAssetMenu(0, 0, true);
            result.DoIfSome(a => a.Parent = parent);
            ImGui.EndPopup();
        }
        return result;
    }

    public static Platform DefaultPlatformWithAssetName(double posX, double posY) => new()
    {
        InstanceName = "Custom_Platform",
        AssetName = "../BattleHill/SK_Small_Plat.png",
        X = posX,
        Y = posY,
        W = 750,
        H = 175,
        ScaleX = 1,
        ScaleY = 1,
    };

    public static Platform DefaultPlatformWithoutAssetName(double posX, double posY) => new()
    {
        InstanceName = "Custom_Platform",
        AssetChildren = [],
        X = posX,
        Y = posY,
        ScaleX = 1,
        ScaleY = 1,
    };
}