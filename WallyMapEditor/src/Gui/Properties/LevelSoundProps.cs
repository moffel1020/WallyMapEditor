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
        propChanged |= ImGuiExt.DragUIntHistory("Interval", ls.Interval, val => ls.Interval = val, cmd, speed: 16);
        propChanged |= ImGuiExt.DragUIntHistory("Delay", ls.Delay, val => ls.Delay = val, cmd, speed: 16);
        propChanged |= ImGuiExt.DragIntHistory("OnlineDelayDiff", ls.OnlineDelayDiff, val => ls.OnlineDelayDiff = val, cmd, speed: 16);
        // do nullable editing because 0 = infinity
        propChanged |= ImGuiExt.DragNullableIntHistory("TotalLoops", ls.TotalLoops == 0 ? null : ls.TotalLoops, 1, val => ls.TotalLoops = val ?? 0, cmd, minValue: 1);
        // badly named prop. is actually "play even if animated backgrounds are off".
        propChanged |= ImGuiExt.CheckboxHistory("IgnoreOnBlurBG", ls.IgnoreOnBlurBG, val => ls.IgnoreOnBlurBG = val, cmd);

        return propChanged;
    }
}