using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowUnimplementedProps()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text("Properties gui not implemented for this object");
        ImGui.PopTextWrapPos();
        return false;
    }
}