using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, CommandHistory cmd, RaylibCanvas? canvas, string? assetDir)
        where T : IDeserializable, ISerializable, IDrawable
    {
        bool propChanged = false;
        ImGui.Text("PlatID: " + ad.PlatID);
        propChanged |= ImGuiExt.DragFloatHistory("X", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("Y", ad.Y, val => ad.Y = val, cmd);

        if (ImGui.CollapsingHeader("Children"))
        {
            foreach (T child in ad.Children)
            {
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd, canvas, assetDir);
                    ImGui.TreePop();
                }
            }
        }

        return propChanged;
    }
}