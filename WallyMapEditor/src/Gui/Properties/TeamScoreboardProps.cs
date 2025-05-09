using WallyMapSpinzor2;
using ImGuiNET;

namespace WallyMapEditor;

public partial class PropertiesWindow
{
    public static bool ShowTeamScoreboardProps(TeamScoreboard ts, EditorLevel level)
    {
        CommandHistory cmd = level.CommandHistory;

        bool propChanged = false;
        ImGui.Text("RedDigitFont: " + ts.RedDigitFont);
        ImGui.Text("BlueDigitFont: " + ts.BlueDigitFont);
        propChanged |= ImGuiExt.DragIntHistory("RedTeamX", ts.RedTeamX, val => ts.RedTeamX = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("BlueTeamX", ts.BlueTeamX, val => ts.BlueTeamX = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("Y", ts.Y, val => ts.Y = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("DoubleDigitsOnesX", ts.DoubleDigitsOnesX, val => ts.DoubleDigitsOnesX = val, cmd);
        propChanged |= ImGuiExt.DragIntHistory("DoubleDigitsTensX", ts.DoubleDigitsTensX, val => ts.DoubleDigitsTensX = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("DoubleDigitsY", ts.DoubleDigitsY, val => ts.DoubleDigitsY = val, cmd);
        propChanged |= ImGuiExt.DragDoubleHistory("DoubleDigitsScale", ts.DoubleDigitsScale, val => ts.DoubleDigitsScale = val, cmd);
        return propChanged;
    }
}