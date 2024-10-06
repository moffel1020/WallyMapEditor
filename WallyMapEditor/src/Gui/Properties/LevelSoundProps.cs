using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowLevelSoundProps(LevelSound ls, CommandHistory cmd, PropertiesWindowData data)
    {
        if (data.Level is not null)
            RemoveButton(ls, cmd, data.Level.Desc.LevelSounds, val => data.Level.Desc.LevelSounds = val);
        ImGui.Separator();

        bool propChanged = false;
        ImGui.Text("SoundEventName: " + ls.SoundEventName);
        propChanged |= ImGuiExt.DragIntHistory("Interval", ls.Interval, val => ls.Interval = val, cmd, minValue: 0, speed: 16);
        propChanged |= ImGuiExt.DragIntHistory("Delay", ls.Delay, val => ls.Delay = val, cmd, minValue: 0, speed: 16);
        return propChanged;
    }
}