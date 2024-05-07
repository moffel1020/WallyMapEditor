using ImGuiNET;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow
{
    public static bool ShowDynamicProps<T>(AbstractDynamic<T> ad, CommandHistory cmd)
        where T : IDeserializable, ISerializable, IDrawable
    {
        bool propChanged = false;
        ImGui.Text("PlatID: " + ad.PlatID);
        propChanged |= ImGuiExt.DragFloatHistory("x", ad.X, val => ad.X = val, cmd);
        propChanged |= ImGuiExt.DragFloatHistory("y", ad.Y, val => ad.Y = val, cmd);

        if (ImGui.CollapsingHeader("Children"))
        {
            foreach (T child in ad.Children)
            {
                if (ImGui.TreeNode($"{child.GetType().Name} {MapOverviewWindow.GetExtraObjectInfo(child)}###dynamicChild{child.GetHashCode()}"))
                {
                    propChanged |= ShowProperties(child, cmd);
                    ImGui.TreePop();
                }
            }
        }

        return propChanged;
    }
}