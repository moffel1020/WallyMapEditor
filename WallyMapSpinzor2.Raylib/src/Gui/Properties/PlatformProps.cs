using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow
{
    public static bool ShowPlatformProps(Platform a, CommandHistory cmd)
    {
        bool propChanged = false;

        string name = a.InstanceName;
        ImGui.InputText("InstanceName", ref name, 64);
        if (name != a.InstanceName)
        {
            cmd.Add(new PropChangeCommand<string>(val => a.InstanceName = val, a.InstanceName, name));
            propChanged = true;
        }

        ImGui.Separator();
        propChanged |= ShowAbstractAssetProps(a, cmd);

        ImGui.Separator();
        ImGui.Text($"Blue: {a.Blue.ToString() ?? "No"}");
        ImGui.Text($"Red: {a.Red.ToString() ?? "No"}");

        if (a.AssetName is null && ImGui.CollapsingHeader("Children"))
        {
            foreach (AbstractAsset child in a.AssetChildren!)
            {
                if (ImGui.TreeNode($"{child.GetType().Name}##{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd);
                    ImGui.TreePop();
                }
            }
        }

        return propChanged;
    }
}