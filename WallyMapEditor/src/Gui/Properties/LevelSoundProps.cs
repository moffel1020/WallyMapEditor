using ImGuiNET;
using WallyMapSpinzor2;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowLevelSoundProps(LevelSound ls, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;
        LevelDesc ld = level.Level.Desc;

        RemoveButton(ls, level);
        ImGui.Separator();

        bool propChanged = false;
        propChanged |= ImGuiExt.InputTextHistory("SoundEventName", ls.SoundEventName, val => ls.SoundEventName = val, cmd);
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