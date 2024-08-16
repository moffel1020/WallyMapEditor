using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowLevelSoundProps(LevelSound ls, CommandHistory cmd)
    {
        bool propChanged = false;
        ImGui.Text("SoundEventName: " + ls.SoundEventName);
        propChanged |= ImGuiExt.DragIntHistory("Interval", ls.Interval, val => ls.Interval = val, cmd, minValue: 0, speed: 16);
        propChanged |= ImGuiExt.DragIntHistory("Delay", ls.Delay, val => ls.Delay = val, cmd, minValue: 0, speed: 16);
        return propChanged;
    }
}