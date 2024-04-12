using ImGuiNET;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

namespace WallyMapSpinzor2.Raylib;

public partial class PropertiesWindow 
{
    private bool _open = true;
    public bool Open { get => _open; set => _open = value; }

    private bool _propChanged = false;

    public void Show(object o, CommandHistory cmd)
    {
        ImGui.Begin($"Properties - {o.GetType().Name}###properties", ref _open);

        if (_propChanged && Rl.IsMouseButtonReleased(MouseButton.Left))
        {
            _propChanged = false;
            cmd.SetAllowMerge(false);
        }

        if (!ShowProperties(o, cmd))
            ImGui.Text("Properties gui not implemented for this object");

        ImGui.End();
    }

    // TODO: for collision and itemspawns, add the ability to change their types
    // TODO: hardcollision should be edited as a shape rather than an individual collision, if they are not a shape they wont work properly ingame
    private bool ShowProperties(object o, CommandHistory cmd) => o switch
    {
        AbstractCollision ac => ShowAbstractCollisionProps(ac, cmd),
        AbstractItemSpawn i => ShowItemSpawnProps(i, cmd),
        Respawn r => ShowRespawnProps(r, cmd),
        _ => false
    };
}