using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowPlatformProps(Platform a, CommandHistory cmd)
    {
        bool propChanged = false;
        string name = a.InstanceName;
        ImGui.InputText("Name", ref name, 64);
        a.InstanceName = name;

        ImGui.Separator();
        propChanged |= ShowAbstractAssetProps(a, cmd);

        ImGui.Separator();
        int? blue = a.Blue;
        int? red = a.Red;
        ImGui.Text($"Blue: {(blue is not null ? blue : "No")}");
        ImGui.Text($"Red: {(red is not null ? red : "No")}");

        if (a.AssetName is null && ImGui.TreeNode("Children"))
        {
            foreach (AbstractAsset child in a.AssetChildren)
            {
                if (ImGui.TreeNode($"{child.GetType().Name}##{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd);
                    ImGui.TreePop();
                }
            }
            ImGui.TreePop();
        }

        return propChanged;
    }
}