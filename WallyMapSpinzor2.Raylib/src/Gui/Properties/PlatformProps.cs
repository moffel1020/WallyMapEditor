using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

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
        }

        ImGui.Separator();
        ImGui.Text($"Blue: {p.Blue?.ToString() ?? "No"}");
        ImGui.Text($"Red: {p.Red?.ToString() ?? "No"}");

        if (p.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            propChanged |= ImGuiExt.EditArrayHistory("", p.AssetChildren!, val => p.AssetChildren = val,
            CreateNew,
            (int index) =>
            {
                if (index != 0)
                    ImGui.Separator();
                AbstractAsset child = p.AssetChildren![index];
                if (ImGui.TreeNode($"{child.GetType().Name}##{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd, data);
                    ImGui.TreePop();
                }
            }, cmd);
        }

        return propChanged;
    }

    private static Maybe<AbstractAsset> CreateNew()
    {
        Maybe<AbstractAsset> result = new();
        if (ImGui.Button("Add new child"))
            ImGui.OpenPopup("AddChild##platform");

        if (ImGui.BeginPopup("AddChild##platform"))
        {
            if (ImGui.MenuItem("Asset"))
                result = DefaultAsset;
            if (ImGui.MenuItem("Platform with AssetName"))
                result = DefaultPlatformWithAssetName;
            if (ImGui.MenuItem("Platform without AssetName"))
                result = DefaultPlatformWithoutAssetName;
            ImGui.EndPopup();
        }
        return result;
    }

    public static Platform DefaultPlatformWithAssetName => new()
    {
        InstanceName = "Custom_Platform",
        AssetName = "../Battlehill/SK_Small_Plat.png",
        X = 0,
        Y = 0,
        W = 750,
        H = 175,
    };

    public static Platform DefaultPlatformWithoutAssetName => new()
    {
        InstanceName = "Custom_Platform",
        AssetChildren = [],
        X = 0,
        Y = 0,
        ScaleX = 1,
        ScaleY = 1,
    };
}