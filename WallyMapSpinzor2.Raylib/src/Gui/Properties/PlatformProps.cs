using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

partial class PropertiesWindow 
{
    public bool ShowPlatformProps(Platform a, CommandHistory cmd)
    {
        string name = a.InstanceName; // dont know if instance name is supposed to be unique? if so maybe enforce when exporting
        ImGui.InputText("Name", ref name, 64);
        a.InstanceName = name;

        ImGui.Separator();
        ShowAbstractAssetProps(a, cmd);

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
                    ShowProperties(child, cmd);
            }
        }

        return true;
    }
}